using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);
        //IQueryable<Order> Query();
        Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, int year);
        Task<int> CountNewOrdersAsync(int? restaurantId, DateTime since);

    }
}
