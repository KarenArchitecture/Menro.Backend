using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Order entities.
    /// Handles revenue, recent orders, and cached user-specific queries.
    /// </summary>
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        public OrderRepository(MenroDbContext context, IMemoryCache cache) : base(context)
        {
            _context = context;
            _cache = cache;
        }

        /* ============================================================
           🔹 Revenue and Analytics
        ============================================================ */

        /// <summary>
        /// Returns total completed revenue globally or for a specific restaurant.
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            var query = _context.Orders.Where(o => o.Status == OrderStatus.Completed);

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        /// <summary>
        /// Returns all completed orders within a given time range.
        /// </summary>
        public async Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to)
        {
            var query = _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Completed &&
                    o.CreatedAt >= from &&
                    o.CreatedAt < to);

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.ToListAsync();
        }

        /* ============================================================
           🔹 Recent Orders Analytics
        ============================================================ */

        /// <summary>
        /// Returns count of recent orders since a given date.
        /// </summary>
        public async Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.CountAsync(o => o.CreatedAt >= since);
        }

        /// <summary>
        /// Returns total revenue of recent orders since a given date.
        /// </summary>
        public async Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            query = query.Where(o => o.CreatedAt >= since);
            return await query.SumAsync(o => (decimal?)o.TotalAmount ?? 0);
        }

        /* ============================================================
           🔹 User-Specific Recent Foods (Cached)
        ============================================================ */

        private const string RecentOrdersKeyPrefix = "UserRecentOrders_";

        private string GetCacheKey(string userId, int count)
            => $"{RecentOrdersKeyPrefix}{userId}_{count}";

        /// <summary>
        /// Returns the most recently ordered foods for a user,
        /// deduplicated by food and sorted by last order date.
        /// Uses in-memory caching for fast repeat access.
        /// </summary>
        public async Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count)
        {
            if (string.IsNullOrWhiteSpace(userId) || count <= 0)
                return new();

            var cacheKey = GetCacheKey(userId, count);

            // ✅ 1) Check cache
            if (_cache.TryGetValue(cacheKey, out List<Food>? cached) && cached != null)
                return cached;

            // 🚀 2) Query DB
            var latestFoodIds = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .SelectMany(o => o.OrderItems.Select(oi => new { o.CreatedAt, oi.FoodId }))
                .GroupBy(x => x.FoodId)
                .Select(g => new { FoodId = g.Key, LastOrderedAt = g.Max(x => x.CreatedAt) })
                .OrderByDescending(x => x.LastOrderedAt)
                .Take(count)
                .Select(x => x.FoodId)
                .ToListAsync();

            if (latestFoodIds.Count == 0)
                return new();

            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => latestFoodIds.Contains(f.Id) && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .ToListAsync();

            // preserve order
            var orderIndex = latestFoodIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            var result = foods
                .OrderBy(f => orderIndex.TryGetValue(f.Id, out var i) ? i : int.MaxValue)
                .ToList();

            // ✅ 3) Cache result
            _cache.Set(cacheKey, result,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                    Priority = CacheItemPriority.Normal
                });

            return result;
        }

        /* ============================================================
           🔄 Cache Invalidation
        ============================================================ */

        /// <summary>
        /// Invalidates cached recent orders for a specific user (for all sizes).
        /// </summary>
        public void InvalidateUserRecentOrders(string userId)
        {
            foreach (var count in new[] { 8, 16, 32 })
                _cache.Remove(GetCacheKey(userId, count));
        }
    }
}
