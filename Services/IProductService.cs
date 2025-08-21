using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;

namespace OrderManagementAPI.Services
{
    public interface IProductService
    {
        Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string category = null);
    }
}