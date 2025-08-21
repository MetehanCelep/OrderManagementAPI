using OrderManagementAPI.Models;

namespace OrderManagementAPI.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<int>> CreateOrderAsync(CreateOrderRequest request);
    }
}