using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing Order entities:
    ///  • Creating orders
    ///  • Revenue and analytics
    ///  • User-specific “recent foods”
    ///  • Retrieving full order graphs for summaries
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /* ============================================================
           ▶️  ORDER CREATION & RETRIEVAL
        ============================================================ */

        /// <summary>
        /// Adds a new Order (with items and extras) to the DbContext.
        /// Note: SaveChangesAsync is NOT called here; use IUnitOfWork for that.
        /// </summary>
        Task AddOrderAsync(Order order);

        /// <summary>
        /// Retrieves an Order with full navigation properties:
        /// OrderItems, Extras (with FoodAddon), Food, and FoodVariant.
        /// Intended for detailed order summaries.
        /// </summary>
        Task<Order?> GetOrderWithDetailsAsync(int orderId);


        /* ============================================================
           💰 REVENUE & ANALYTICS
        ============================================================ */

        /// <summary>
        /// Returns the total revenue of completed orders.
        /// If restaurantId is null, returns global revenue across all restaurants.
        /// </summary>
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);

        /// <summary>
        /// Returns all completed orders inside a given time window.
        /// </summary>
        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to);

        /// <summary>
        /// Returns count of orders placed since a given date.
        /// Optionally filtered by restaurant.
        /// </summary>
        Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since);

        /// <summary>
        /// Returns total revenue of orders placed since a given date.
        /// Optionally filtered by restaurant.
        /// </summary>
        Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since);


        /* ============================================================
           👤 USER-SPECIFIC RECENT FOODS (CACHED)
        ============================================================ */

        /// <summary>
        /// Returns the most recently ordered foods for a user,
        /// deduplicated by food and sorted by last order time.
        /// Uses in-memory caching for fast repeat access.
        /// </summary>
        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count);

        /// <summary>
        /// Invalidates cached recent foods for the given user.
        /// </summary>
        void InvalidateUserRecentOrders(string userId);
    }
}
