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

        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);

        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, int year);

        Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since);
        Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since);


        Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count);
    }
}
