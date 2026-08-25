using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<int> GetNextRestaurantOrderNumberAsync(int restaurantId, CancellationToken ct = default)
        {
            var last = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.RestaurantOrderNumber)
                .Select(o => (int?)o.RestaurantOrderNumber)
                .FirstOrDefaultAsync(ct);

            return (last ?? 0) + 1;
        }

        public async Task AddOrderAsync(Order order, CancellationToken ct = default)
        {
            await _context.Orders.AddAsync(order, ct);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken ct = default)
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
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task<Order?> GetPublicOrderDetailsAsync(int orderId, CancellationToken ct = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                        .ThenInclude(f => f.Ratings)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodVariant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Extras)
                        .ThenInclude(e => e.FoodAddon)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task<List<Food>> GetUserFrequentFoodsForRestaurantAsync(string userId, int restaurantId, int count, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId) || count <= 0)
                return new List<Food>();

            var ranked = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.UserId == userId && oi.Order.RestaurantId == restaurantId)
                .GroupBy(oi => oi.FoodId)
                .Select(g => new { FoodId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .ThenByDescending(x => x.FoodId)
                .Take(count)
                .ToListAsync(ct);

            if (ranked.Count == 0)
                return new List<Food>();

            var ids = ranked.Select(x => x.FoodId).ToList();

            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => ids.Contains(f.Id) && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Variants.Where(v => !v.IsDeleted && v.IsAvailable))
                    .ThenInclude(v => v.Addons.Where(a => !a.IsDeleted))
                .AsSplitQuery()
                .ToListAsync(ct);

            var rankLookup = ranked.Select((x, idx) => new { x.FoodId, idx }).ToDictionary(x => x.FoodId, x => x.idx);

            return foods
                .OrderBy(f => rankLookup.TryGetValue(f.Id, out var pos) ? pos : int.MaxValue)
                .ToList();
        }

        /* ============================================================
           💰 AdminPanel
        ============================================================ */

        public async Task<int> GetTotalRevenueAsync(int? restaurantId = null, CancellationToken ct = default)
        {
            var query = _context.Orders.Where(o => o.Status == OrderStatus.Completed);
            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }
            return await query.SumAsync(o => o.TotalPrice, ct);
        }

        public async Task<int> GetRecentOrdersRevenueAsync(int? restaurantId, DateTime since, CancellationToken ct = default)
        {
            var query = _context.Orders.AsQueryable();
            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }
            return await query
                .Where(o => o.CreatedAt >= since && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalPrice, ct);
        }

        public async Task<List<Order>> GetCompletedOrdersAsync(int? restaurantId, DateTime from, DateTime to, CancellationToken ct = default)
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
                .ToListAsync(ct);
        }

        public async Task<int> GetRecentOrdersCountAsync(int? restaurantId, DateTime since, CancellationToken ct = default)
        {
            var query = _context.Orders.AsQueryable();

            if (restaurantId.HasValue)
            {
                int id = restaurantId.Value;
                query = query.Where(o => o.RestaurantId == id);
            }

            return await query.CountAsync(o => o.CreatedAt >= since, ct);
        }

        public async Task<List<Order>> GetActiveOrdersAsync(int restaurantId, CancellationToken ct = default)
        {
            var activeStatuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.Confirmed,
                OrderStatus.Delivered,
                OrderStatus.Paid
            };

            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && activeStatuses.Contains(o.Status))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Order>> GetOrderHistoryAsync(int restaurantId, CancellationToken ct = default)
        {
            var historyStatuses = new[]
            {
                OrderStatus.Cancelled,
                OrderStatus.Completed
            };

            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && historyStatuses.Contains(o.Status))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Order?> GetOrderDetailsAsync(int restaurantId, int orderId, CancellationToken ct = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && o.Id == orderId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Extras)
                        .ThenInclude(e => e.FoodAddon)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Order?> GetForUpdateAsync(int restaurantId, int orderId, CancellationToken ct = default)
        {
            // Tracking query (بدون AsNoTracking)
            return await _context.Orders
                .Where(o => o.RestaurantId == restaurantId && o.Id == orderId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<int> CountOrdersForRestaurantOnDateAsync(int restaurantId, DateTime dayStartUtc, DateTime dayEndUtc, CancellationToken ct = default)
        {
            return await _context.Orders
                .Where(o => o.RestaurantId == restaurantId && o.CreatedAt >= dayStartUtc && o.CreatedAt < dayEndUtc)
                .CountAsync(ct);
        }

        public async Task<List<Order>> SearchOrdersByInvoiceAsync(int restaurantId, string query, int take, CancellationToken ct = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.RestaurantId == restaurantId && o.InvoiceNumber.Contains(query))
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct) > 0;


        /* ============================================================
           👤 USER-SPECIFIC RECENT FOODS (CACHED)
        ============================================================ */

        private const string RecentOrdersKeyPrefix = "UserRecentOrders_";

        // 🔧 Cache ONE list per user, not one per distinct "count" value. The
        // old scheme keyed the cache as "{prefix}{userId}_{count}" and only
        // invalidated a hardcoded set of counts (8/16/32). The Home page
        // actually requests count=9 (PREVIEW_COUNT+1, to detect "has more"),
        // which was never in that hardcoded set — so its cache entry never
        // got cleared on a new order and could silently go stale for its
        // full 3-minute TTL. Caching a single "top N" list per user and
        // slicing to whatever count was asked for AFTER reading the cache
        // removes this whole class of bug: invalidation is one key removal,
        // and it doesn't matter what count any caller asks for in the future.
        private const int MaxCachedRecentFoods = 32; // covers every caller's clamp today (Home: <=32, browse: separate cursor method, uncached)

        private string GetCacheKey(string userId) => $"{RecentOrdersKeyPrefix}{userId}";

        public async Task<List<Food>> GetUserRecentlyOrderedFoodsAsync(string userId, int count, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId) || count <= 0)
                return new List<Food>();

            var cacheKey = GetCacheKey(userId);

            // 1) Try cache — holds up to MaxCachedRecentFoods, already ordered by recency
            if (!_cache.TryGetValue(cacheKey, out List<Food>? cached) || cached == null)
            {
                // 2) Query latest food ids from order history (always fetch the
                // max we'd ever cache, regardless of what this particular
                // caller asked for, so the cached list can serve any count up
                // to MaxCachedRecentFoods without a separate DB round trip)
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
                    .Take(MaxCachedRecentFoods)
                    .Select(x => x.FoodId)
                    .ToListAsync(ct);

                if (latestFoodIds.Count == 0)
                {
                    cached = new List<Food>();
                }
                else
                {
                    // 3) Load foods themselves
                    var foods = await _context.Foods
                        .AsNoTracking()
                        .Where(f => latestFoodIds.Contains(f.Id) && f.IsAvailable && !f.IsDeleted)
                        .Include(f => f.Ratings)
                        .Include(f => f.Restaurant)
                        .ToListAsync(ct);

                    // Preserve original order
                    var indexLookup = latestFoodIds
                        .Select((id, idx) => new { id, idx })
                        .ToDictionary(x => x.id, x => x.idx);

                    cached = foods
                        .OrderBy(f => indexLookup.TryGetValue(f.Id, out var pos) ? pos : int.MaxValue)
                        .ToList();
                }

                // 4) Cache result
                _cache.Set(
                    cacheKey,
                    cached,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                        Priority = CacheItemPriority.Normal
                    });
            }

            // 5) Slice to whatever this specific caller asked for, from the
            // (now guaranteed fresh-or-cached) full list.
            return cached.Take(Math.Min(count, MaxCachedRecentFoods)).ToList();
        }

        private static string EncodeCursor(DateTime dt, int foodId)
        {
            var raw = $"{dt.Ticks}:{foodId}";
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
            // url-safe
            return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool TryDecodeCursor(string? cursor, out DateTime dt, out int foodId)
        {
            dt = default;
            foodId = default;

            if (string.IsNullOrWhiteSpace(cursor)) return false;

            try
            {
                var b64 = cursor.Replace('-', '+').Replace('_', '/');
                // pad
                switch (b64.Length % 4)
                {
                    case 2: b64 += "=="; break;
                    case 3: b64 += "="; break;
                }

                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                var parts = raw.Split(':');
                if (parts.Length != 2) return false;

                if (!long.TryParse(parts[0], out var ticks)) return false;
                if (!int.TryParse(parts[1], out var id)) return false;

                dt = new DateTime(ticks);
                foodId = id;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(List<Food> Foods, string? NextCursor, bool HasMore)> GetUserRecentlyOrderedFoodsCursorAsync(
            string userId, int take, string? cursor, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId) || take <= 0)
                return (new List<Food>(), null, false);

            take = Math.Clamp(take, 1, 24);

            var baseQuery = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .SelectMany(o => o.OrderItems.Select(oi => new { oi.FoodId, o.CreatedAt }))
                .GroupBy(x => x.FoodId)
                .Select(g => new
                {
                    FoodId = g.Key,
                    LastOrderedAt = g.Max(x => x.CreatedAt)
                });

            // cursor filter (desc order): load items "after" the cursor
            if (TryDecodeCursor(cursor, out var cTime, out var cFoodId))
            {
                baseQuery = baseQuery.Where(x =>
                    x.LastOrderedAt < cTime ||
                    (x.LastOrderedAt == cTime && x.FoodId < cFoodId)
                );
            }

            var rows = await baseQuery
                .OrderByDescending(x => x.LastOrderedAt)
                .ThenByDescending(x => x.FoodId)
                .Take(take + 1)
                .ToListAsync(ct);

            var hasMore = rows.Count > take;
            var pageRows = rows.Take(take).ToList();

            if (pageRows.Count == 0)
                return (new List<Food>(), null, false);

            var nextCursor = hasMore
                ? EncodeCursor(pageRows.Last().LastOrderedAt, pageRows.Last().FoodId)
                : null;

            var ids = pageRows.Select(x => x.FoodId).ToList();

            var foods = await _context.Foods
                .AsNoTracking()
                .Where(f => ids.Contains(f.Id) && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .ToListAsync(ct);

            // preserve ids order
            var index = ids.Select((id, idx) => new { id, idx }).ToDictionary(x => x.id, x => x.idx);

            var result = foods
                .OrderBy(f => index.TryGetValue(f.Id, out var pos) ? pos : int.MaxValue)
                .ToList();

            return (result, nextCursor, hasMore);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }

        /* ============================================================
           🔄 CACHE INVALIDATION
        ============================================================ */

        public void InvalidateUserRecentOrders(string userId)
        {
            // 🔧 Now a single key removal — no more guessing which "count"
            // values might be cached.
            _cache.Remove(GetCacheKey(userId));
        }
    }
}