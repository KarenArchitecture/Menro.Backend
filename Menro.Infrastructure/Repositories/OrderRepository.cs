using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Infrastructure.Data;
using Menro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Menro.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;

        public OrderRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }
        //public IQueryable<Order> Query()
        //{
        //    return _context.Orders.AsQueryable();
        //}
        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            var query = _context.Orders.Where(o => o.Status == OrderStatus.Completed);
            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);
            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, int year)
        {
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt.Year == year);

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.ToListAsync();
        }
        public async Task<int> CountNewOrdersAsync(int? restaurantId, DateTime since)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.CountAsync(o => o.CreatedAt >= since);
        }

    }
}
