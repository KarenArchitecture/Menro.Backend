using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        public OrderRepository(MenroDbContext context, IMemoryCache cache)
            : base(context)
        {
            _context = context;
            _cache = cache;
        }


        /* ============================================================
           ▶️  ORDER CREATION & RETRIEVAL
        ============================================================ */

        public async Task<int> GetNextRestaurantOrderNumberAsync(int restaurantId)
        {
            var last = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.RestaurantOrderNumber)
                .Select(o => (int?)o.RestaurantOrderNumber)
                .FirstOrDefaultAsync();

            return (last ?? 0) + 1;
        }


        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                // Items + Food
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                // Items + Variant
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodVariant)
                // Items + Extras + FoodAddon
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Extras)
                        .ThenInclude(e => e.FoodAddon)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }


        /* ============================================================
           💰 AdminPanel
        ============================================================ */
        
        /* dashboard stats */

        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Completed);

            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }

            return await query.SumAsync(o => o.TotalPrice);
        }

        public async Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to)
        {
            var query = _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Completed &&
                    o.CreatedAt >= from &&
                    o.CreatedAt < to);

            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }

            return await query.CountAsync(o => o.CreatedAt >= since);
        }

        public async Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }

            return await query
                .Where(o => o.CreatedAt >= since && o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalPrice ?? 0m);
        }

        /* order management */

        public async Task<List<Order>> GetActiveOrdersAsync(int restaurantId)
        {
            var activeStatuses = new[]
            {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Paid
        };

            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && activeStatuses.Contains(o.Status))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrderHistoryAsync(int restaurantId)
        {
            var historyStatuses = new[]
            {
            OrderStatus.Cancelled,
            OrderStatus.Delivered,
            OrderStatus.Completed
        };

            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && historyStatuses.Contains(o.Status))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        /* ============================================================
           👤 USER-SPECIFIC RECENT FOODS (CACHED)
        ============================================================ */

        private const string RecentOrdersKeyPrefix = "UserRecentOrders_";

        private string GetCacheKey(string userId, int count)
            => $"{RecentOrdersKeyPrefix}{userId}_{count}";

        public async Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count)
        {
            if (string.IsNullOrWhiteSpace(userId) || count <= 0)
                return new List<Food>();

            var cacheKey = GetCacheKey(userId, count);

            // 1) Try cache
            if (_cache.TryGetValue(cacheKey, out List<Food>? cached) && cached != null)
                return cached;

            // 2) Query latest food ids from order history
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
                return new List<Food>();

            // 3) Load foods themselves
            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => latestFoodIds.Contains(f.Id) && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .ToListAsync();

            // Preserve original order
            var indexLookup = latestFoodIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            var result = foods
                .OrderBy(f => indexLookup.TryGetValue(f.Id, out var pos) ? pos : int.MaxValue)
                .ToList();

            // 4) Cache result
            _cache.Set(
                cacheKey,
                result,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                    Priority = CacheItemPriority.Normal
                });

            return result;
        }


        /* ============================================================
           🔄 CACHE INVALIDATION
        ============================================================ */

        public void InvalidateUserRecentOrders(string userId)
        {
            // If you use only certain "count" sizes, you can clear those.
            foreach (var count in new[] { 8, 16, 32 })
            {
                _cache.Remove(GetCacheKey(userId, count));
            }
        }
    }
}
