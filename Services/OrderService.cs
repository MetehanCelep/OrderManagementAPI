using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Entities;
using OrderManagementAPI.Models;
using OrderManagementAPI.Services;

namespace OrderManagementAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IRabbitMqService rabbitMqService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public async Task<ApiResponse<int>> CreateOrderAsync(CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Calculate total amount
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var productDetail in request.Products)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == productDetail.ProductId && p.Status == true);

                    if (product == null)
                    {
                        return ApiResponse<int>.Failed($"�r�n bulunamad�: {productDetail.ProductId}", "PRODUCT_NOT_FOUND");
                    }

                    var lineTotal = productDetail.UnitPrice * productDetail.Amount;
                    totalAmount += lineTotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = productDetail.ProductId,
                        UnitPrice = productDetail.UnitPrice,
                        Amount = productDetail.Amount
                    });
                }

                // Create order
                var order = new Order
                {
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerGSM = request.CustomerGSM,
                    TotalAmount = totalAmount,
                    CreateDate = DateTime.Now,
                    OrderDetails = orderDetails
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Send email to queue
                var emailData = new
                {
                    Email = request.CustomerEmail,
                    CustomerName = request.CustomerName,
                    OrderId = order.Id,
                    TotalAmount = totalAmount
                };

                _rabbitMqService.SendMessage("SendMail", emailData);

                await transaction.CommitAsync();

                _logger.LogInformation($"Order created successfully. OrderId: {order.Id}");
                return ApiResponse<int>.Success(order.Id, "Sipari� ba�ar�yla olu�turuldu");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order");
                return ApiResponse<int>.Failed("Sipari� olu�turulurken hata olu�tu", "ORDER_ERROR");
            }
        }
    }
}