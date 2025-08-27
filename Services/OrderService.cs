using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Entities;
using OrderManagementAPI.Models;

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
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var productDetail in request.Products)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == productDetail.ProductId && p.Status == true);

                    if (product == null)
                    {
                        return ApiResponse<int>.Failed($"Ürün bulunamadı: {productDetail.ProductId}", "PRODUCT_NOT_FOUND");
                    }

                    var unitPrice = product.UnitPrice; // Fiyat DB'den
                    var lineTotal = unitPrice * productDetail.Amount;
                    totalAmount += lineTotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = productDetail.ProductId,
                        UnitPrice = unitPrice,
                        Amount = productDetail.Amount
                    });
                }

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
                return ApiResponse<int>.Success(order.Id, "Sipariş başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order");
                return ApiResponse<int>.Failed("Sipariş oluşturulurken hata oluştu", "ORDER_ERROR");
            }
        }
    }
}
