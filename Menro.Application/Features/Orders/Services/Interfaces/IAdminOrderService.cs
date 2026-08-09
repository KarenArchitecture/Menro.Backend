using Menro.Application.Features.Orders.DTOs;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IAdminOrderService
    {
        Task<List<MonthlySalesDto>> GetMonthlySalesRawAsync(int? restaurantId = null);
        Task<int> GetTotalRevenueAsync(int? restaurantId = null);
        Task<int> GetRecentOrdersCountAsync(int? restaurantId = null, int daysBack = 30);
        Task<int> GetRecentOrdersRevenueAsync(int? restaurantId = null, int daysBack = 0);


        /* order management */
        Task<List<AdminOrderListItemDto>> GetActiveOrdersAsync(int restaurantId);
        Task<List<AdminOrderListItemDto>> GetOrderHistoryAsync(int restaurantId);
        Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(int restaurantId, int orderId);
        Task<List<AdminOrderListItemDto>> SearchOrdersAsync(int restaurantId, string query);

        // manage order status
        Task<OrderStatus?> AdvanceStatusAsync(int restaurantId, int orderId);
        Task<OrderStatus?> CancelOrderAsync(int restaurantId, int orderId);

    }
}
