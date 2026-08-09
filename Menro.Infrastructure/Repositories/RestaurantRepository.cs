using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        public RestaurantRepository(MenroDbContext context, IMemoryCache cache)
            : base(context)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            return await _context.Restaurants
                .Include(r => r.OwnerUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetRestaurantName(int restaurantId)
        {
            string cacheKey = $"RestaurantName:{restaurantId}";

            if (_cache.TryGetValue(cacheKey, out string cached))
                return cached;

            var name = await _context.Restaurants
                .Where(r => r.Id == restaurantId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? "منرو";

            _cache.Set(cacheKey, name, TimeSpan.FromMinutes(30));
            return name;
        }

        public async Task<List<Restaurant>> GetActiveApprovedWithDetailsPageAsync(int take, int? cursorId)
        {
            var query = _context.Restaurants
                .Where(r => r.IsActive && !r.IsDeleted && r.Status == RestaurantStatus.Approved)
                .OrderByDescending(r => r.Id)
                .Include(r => r.Ratings)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Discounts)   // ✅ FIX ADDED
                .AsNoTracking();

            if (cursorId.HasValue)
                query = query.Where(r => r.Id < cursorId.Value);

            return await query.Take(take + 1).ToListAsync();
        }

        public async Task<List<Restaurant>> GetRandomActiveApprovedWithDetailsAsync(int count)
        {
            const string cacheKey = "RandomRestaurants";

            if (_cache.TryGetValue(cacheKey, out List<Restaurant> cached))
            {
                return cached.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
            }

            var restaurants = await _context.Restaurants
                .AsNoTracking()
                .Where(r => r.IsActive && !r.IsDeleted && r.Status == RestaurantStatus.Approved)
                .OrderBy(r => EF.Functions.Random())
                .Take(count * 2)
                .Include(r => r.Ratings)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Discounts)
                // 🔧 Ratings + Discounts are sibling one-to-many collections on the
                // same root. Without this, EF joins both into ONE query, so every
                // restaurant row gets duplicated once per (rating × discount)
                // combination before EF de-dupes it client-side — classic
                // Cartesian explosion. AsSplitQuery() issues Ratings and Discounts
                // as separate SQL queries instead, so the row count stays sane.
                .AsSplitQuery()
                .ToListAsync();

            _cache.Set(cacheKey, restaurants, TimeSpan.FromMinutes(5));

            return restaurants
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        public async Task<bool> IncrementBannerImpressionAsync(int bannerId)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [RestaurantAdBanners]
                SET [ConsumedViews] = [ConsumedViews] + 1
                WHERE [Id] = {bannerId}
                  AND [IsPaused] = 0
                  AND [StartDate] <= GETUTCDATE()
                  AND [EndDate] >= GETUTCDATE()
                  AND ([PurchasedViews] = 0 OR [ConsumedViews] < [PurchasedViews]);
            ");

            return rows > 0;
        }

        public async Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new();

            var latestByRestaurant = await _context.Orders
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.RestaurantId)
                .Select(g => new
                {
                    RestaurantId = g.Key,
                    LastOrderAt = g.Max(o => o.CreatedAt)
                })
                .ToListAsync();

            if (!latestByRestaurant.Any())
                return new();

            var ids = latestByRestaurant.Select(x => x.RestaurantId).ToList();

            var restaurants = await _context.Restaurants
                .Where(r => ids.Contains(r.Id))
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)   // (optional but safe) 
                .ToListAsync();

            var map = latestByRestaurant.ToDictionary(x => x.RestaurantId, x => x.LastOrderAt);

            return restaurants
                .OrderByDescending(r => map.TryGetValue(r.Id, out var t) ? t : DateTime.MinValue)
                .ToList();
        }

        public async Task<Restaurant?> GetRestaurantBannerBySlugAsync(string slug)
        {
            string cacheKey = $"RestaurantBanner:{slug}";

            if (_cache.TryGetValue(cacheKey, out Restaurant cached))
                return cached;

            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)   // ✅ FIX ADDED
                .FirstOrDefaultAsync(r =>
                    r.Slug == slug &&
                    r.IsActive &&
                    !r.IsDeleted &&
                    r.Status == RestaurantStatus.Approved);

            if (restaurant != null)
                _cache.Set(cacheKey, restaurant, TimeSpan.FromMinutes(10));

            return restaurant;
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeRestaurantId = null)
        {
            var query = _context.Restaurants.Where(r => r.Slug == slug);
            if (excludeRestaurantId.HasValue)
                query = query.Where(r => r.Id != excludeRestaurantId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            string cacheKey = $"RestaurantIdByUser:{userId}";

            if (_cache.TryGetValue(cacheKey, out int cached))
                return cached;

            var id = await _context.Restaurants
                .Where(r => r.OwnerUserId == userId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            _cache.Set(cacheKey, id, TimeSpan.FromMinutes(30));
            return id;
        }

        public void InvalidateFeaturedRestaurants() => _cache.Remove("FeaturedRestaurants");
        public void InvalidateRandomRestaurants() => _cache.Remove("RandomRestaurants");
        public void InvalidateRestaurantBanner(string slug) => _cache.Remove($"RestaurantBanner:{slug}");
        public void InvalidateRestaurantIdByUser(string userId) => _cache.Remove($"RestaurantIdByUser:{userId}");
        public void InvalidateBannerIds() => _cache.Remove("LiveBannerIds");


        // admin panel =>
        public async Task<List<Restaurant>> GetRestaurantsListForAdminAsync(RestaurantStatus status)
        {
            var query = _context.Restaurants
                .Include(r => r.OwnerUser)
                .Include(r => r.Discounts)
                .AsQueryable();

            query = status switch
            {
                RestaurantStatus.Approved => query.Where(r => r.Status == RestaurantStatus.Approved),
                RestaurantStatus.Pending => query.Where(r => r.Status != RestaurantStatus.Approved && r.Status != RestaurantStatus.Rejected),
                RestaurantStatus.Rejected => query.Where(r => r.Status == RestaurantStatus.Rejected),
                _ => query
            };

            return await query
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }
        public async Task<Restaurant?> GetRestaurantDetailsForAdminAsync(int id)
        {
            return await _context.Restaurants
                .Include(r => r.OwnerUser)
                .Include(r => r.RestaurantCategory)
                //.Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public IQueryable<Restaurant> QueryForAdmin(RestaurantStatus? status, string? search, int? categoryId = null)
        {
            var query = _context.Restaurants.AsQueryable();
            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r => r.Name.Contains(term) || r.ContactNumber.Contains(term));
            }
            if (categoryId.HasValue)
                query = query.Where(r => r.RestaurantCategoryId == categoryId.Value);
            return query;
        }
        public async Task<Restaurant?> GetRestaurantProfileAsync(int restaurantId)
        {
            return await _context.Restaurants
                .Include(r => r.OwnerUser)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(r => r.Discounts)   // optional consistency
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
        }
    }
}