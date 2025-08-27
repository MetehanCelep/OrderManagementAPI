using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;
using OrderManagementAPI.Services;

namespace OrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts([FromQuery] string category = null)
        {
            try
            {
                _logger.LogInformation("GetProducts called with category: {Category}", category ?? "all");

                var result = await _productService.GetProductsAsync(category);

                if (result.Status == Status.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProducts endpoint");
                return StatusCode(500, ApiResponse<List<ProductDto>>.Failed("Sunucu hatası", "INTERNAL_ERROR"));
            }
        }
    }
}
