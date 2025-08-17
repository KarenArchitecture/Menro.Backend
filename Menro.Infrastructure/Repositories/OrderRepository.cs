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


namespace Menro.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;

        public OrderRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<Order> Query()
        {
            return _context.Orders.AsQueryable();
        }
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }

    }
}
