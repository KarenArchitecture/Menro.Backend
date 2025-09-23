using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;

        public OrderRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Completed);

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, int year)
        {
            var query = _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Completed &&
                    o.CreatedAt.Year == year);

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

        /// <summary>
        /// Returns the most recently ordered foods for a user, deduped by Food,
        /// ordered by last order date descending.
        /// </summary>
        public async Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count)
        {
            if (string.IsNullOrWhiteSpace(userId) || count <= 0)
                return new();

            // 1) Collect the latest order time per FoodId for this user
            var latestFoodIds = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .SelectMany(o => o.OrderItems.Select(oi => new { o.CreatedAt, oi.FoodId }))
                .GroupBy(x => x.FoodId)
                .Select(g => new
                {
                    FoodId = g.Key,
                    LastOrderedAt = g.Max(x => x.CreatedAt)
                })
                .OrderByDescending(x => x.LastOrderedAt)
                .Take(count)
                .Select(x => x.FoodId)
                .ToListAsync();

            if (latestFoodIds.Count == 0)
                return new();

            // 2) Load foods with only necessary includes
            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => latestFoodIds.Contains(f.Id) &&
                            f.IsAvailable &&
                            !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .ToListAsync();

            // 3) Preserve ordering (most recent first)
            var orderIndex = latestFoodIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            return foods
                .OrderBy(f => orderIndex.TryGetValue(f.Id, out var i) ? i : int.MaxValue)
                .ToList();
        }
    }
}
