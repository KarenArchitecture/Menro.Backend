using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        /* ============================================================
           ▶️  ORDER CREATION & RETRIEVAL
        ============================================================ */

        Task<int> GetNextRestaurantOrderNumberAsync(int restaurantId, CancellationToken ct = default);

        Task AddOrderAsync(Order order, CancellationToken ct = default);

        Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken ct = default);

        Task<Order?> GetPublicOrderDetailsAsync(int orderId, CancellationToken ct = default);

        Task<List<Food>> GetUserFrequentFoodsForRestaurantAsync(string userId, int restaurantId, int count, CancellationToken ct = default);


        /* ============================================================
           💰 AdminPanel
        ============================================================ */

        /* dashboard stats */

        Task<int> GetTotalRevenueAsync(int? restaurantId = null, CancellationToken ct = default);

        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to, CancellationToken ct = default);

        Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since, CancellationToken ct = default);

        Task<int> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since, CancellationToken ct = default);

        /* order management */

        Task<List<Order>> GetActiveOrdersAsync(int restaurantId, CancellationToken ct = default);

        Task<List<Order>> GetOrderHistoryAsync(int restaurantId, CancellationToken ct = default);

        Task<Order?> GetOrderDetailsAsync(int restaurantId, int orderId, CancellationToken ct = default);

        Task<Order?> GetForUpdateAsync(int restaurantId, int orderId, CancellationToken ct = default);

        Task<int> CountOrdersForRestaurantOnDateAsync(int restaurantId, DateTime dayStartUtc, DateTime dayEndUtc, CancellationToken ct = default);

        Task<List<Order>> SearchOrdersByInvoiceAsync(int restaurantId, string query, int take, CancellationToken ct = default);

        Task<bool> SaveChangesAsync(CancellationToken ct = default);


        /* ============================================================
           👤 USER-SPECIFIC RECENT FOODS (CACHED)
        ============================================================ */

        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count, CancellationToken ct = default);

        void InvalidateUserRecentOrders(string userId);

        Task<(List<Food> Foods, string? NextCursor, bool HasMore)> GetUserRecentlyOrderedFoodsCursorAsync(
            string userId, int take, string? cursor, CancellationToken ct = default);

        Task<List<Order>> GetUserOrdersAsync(string userId, CancellationToken ct = default);
    }
}