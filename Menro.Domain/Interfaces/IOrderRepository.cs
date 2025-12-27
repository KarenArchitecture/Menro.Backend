using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        /* ============================================================
           ▶️  ORDER CREATION & RETRIEVAL
        ============================================================ */

        Task<int> GetNextRestaurantOrderNumberAsync(int restaurantId);

        Task AddOrderAsync(Order order);

        Task<Order?> GetOrderWithDetailsAsync(int orderId);


        /* ============================================================
           💰 REVENUE & ANALYTICS
        ============================================================ */

        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);

        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to);

        Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since);

        Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since);


        /* ============================================================
           👤 USER-SPECIFIC RECENT FOODS (CACHED)
        ============================================================ */

        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count);

        void InvalidateUserRecentOrders(string userId);
    }
}
