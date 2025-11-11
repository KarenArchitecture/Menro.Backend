using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing Order entities,
    /// including analytics, recent items, and cached queries.
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /* ============================================================
           🔹 Revenue & Analytics
        ============================================================ */

        /// <summary>
        /// Returns total revenue (completed orders) for a specific restaurant or all restaurants.
        /// </summary>
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);

        /// <summary>
        /// Returns completed orders within a specific date range.
        /// </summary>
        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to);

        /// <summary>
        /// Returns count of orders placed since a given date.
        /// </summary>
        Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since);

        /// <summary>
        /// Returns total revenue from orders placed since a given date.
        /// </summary>
        Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since);

        /* ============================================================
           🔹 User-Specific Recent Orders (Cached)
        ============================================================ */

        /// <summary>
        /// Returns most recently ordered foods for a specific user (deduped and ordered).
        /// </summary>
        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count);

        /// <summary>
        /// Invalidates cached user recent orders (for all sizes).
        /// </summary>
        void InvalidateUserRecentOrders(string userId);
    }
}
