// Menro.Domain/Interfaces/IOrderRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Order aggregate read operations (revenue, counts, recent items, etc.)
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Total revenue from completed/paid orders (optionally scoped to a restaurant).
        /// </summary>
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);

        /// <summary>
        /// Completed orders for a given year (optionally scoped to a restaurant).
        /// </summary>
        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, int year);

        /// <summary>
        /// Count new orders since a given moment (optionally scoped to a restaurant).
        /// </summary>
        Task<int> CountNewOrdersAsync(int? restaurantId, DateTime since);

        /// <summary>
        /// For a given user, returns their most recently ordered foods across all restaurants,
        /// deduped by Food and ordered by last time they were ordered (desc).
        /// </summary>
        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count);
    }
}
