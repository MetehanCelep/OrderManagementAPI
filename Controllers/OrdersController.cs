using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.Models;
using OrderManagementAPI.Services;

namespace OrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<int>.Failed(
                        $"Validation errors: {string.Join(", ", errors)}", 
                        "VALIDATION_ERROR"));
                }

                _logger.LogInformation("CreateOrder called for customer: {Email}", request.CustomerEmail);

                var result = await _orderService.CreateOrderAsync(request);

                if (result.Status == Status.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateOrder endpoint");
                return StatusCode(500, ApiResponse<int>.Failed("Sunucu hatası", "INTERNAL_ERROR"));
            }
        }
    }
}
