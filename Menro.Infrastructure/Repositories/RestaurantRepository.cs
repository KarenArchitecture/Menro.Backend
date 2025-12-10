using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        public RestaurantRepository(MenroDbContext context, IMemoryCache cache) : base(context)
        {
            _context = context;
            _cache = cache;
        }



        /* ============================================================
                                    CRUDS
        ============================================================ */
        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            return await _context.Restaurants.Include(r => r.OwnerUser).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        /* ============================================================
           🔹 Basic name lookup
        ============================================================ */

        /// <summary>
        /// Retrieves a restaurant by ID.
        /// </summary>
        //public async Task<Restaurant?> GetByIdAsync(int id)
        //{
        //    return await _context.Restaurants
        //        .FirstOrDefaultAsync(r => r.Id == id);
        //}

        public async Task<string> GetRestaurantName(int restaurantId)
        {
            string cacheKey = $"RestaurantName:{restaurantId}";
            if (_cache.TryGetValue(cacheKey, out string cachedName))
                return cachedName;

            var name = await _context.Restaurants
                .Where(r => r.Id == restaurantId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? "منرو";

            _cache.Set(cacheKey, name, TimeSpan.FromMinutes(30));
            return name;
        }



        /* ============================================================
           🔹 Featured restaurants (carousel)
        ============================================================ */
        public async Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync()
        {
            const string cacheKey = "FeaturedRestaurants";
            if (_cache.TryGetValue(cacheKey, out List<Restaurant> cached))
                return cached;

            var result = await _context.Restaurants
                .Where(r => r.IsFeatured && r.IsActive && r.IsApproved && !string.IsNullOrEmpty(r.CarouselImageUrl))
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        /* ============================================================
           🔹 Home Page - Random Restaurant Cards
        ============================================================ */
        public async Task<List<Restaurant>> GetRandomActiveApprovedWithDetailsAsync(int count)
        {
            const string cacheKey = "RandomRestaurants";
            if (_cache.TryGetValue(cacheKey, out List<Restaurant> cached))
            {
                return cached.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
            }

            var restaurants = await _context.Restaurants
                .Where(r => r.IsActive && r.IsApproved)
                .OrderBy(r => EF.Functions.Random())
                .Take(count * 2)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .Include(r => r.RestaurantCategory)
                .AsNoTracking()
                .ToListAsync();

            _cache.Set(cacheKey, restaurants, TimeSpan.FromMinutes(5));
            return restaurants.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        /* ============================================================
           🔹 Home Page - Advertisement Banners
        ============================================================ */
        public async Task<RestaurantAdBanner?> GetRandomLiveAdBannerAsync(IEnumerable<int> excludeIds)
        {
            var now = DateTime.UtcNow;
            var excludes = excludeIds?.ToList() ?? new();

            const string cacheKey = "LiveBannerIds";
            if (!_cache.TryGetValue(cacheKey, out List<int> bannerIds))
            {
                bannerIds = await _context.RestaurantAdBanners
                    .Where(b =>
                        b.StartDate <= now &&
                        b.EndDate >= now &&
                        !b.IsPaused &&
                        (b.PurchasedViews == 0 || b.ConsumedViews < b.PurchasedViews) &&
                        b.Restaurant.IsActive &&
                        b.Restaurant.IsApproved)
                    .Select(b => b.Id)
                    .ToListAsync();

                _cache.Set(cacheKey, bannerIds, TimeSpan.FromSeconds(5));
            }

            var availableIds = bannerIds.Except(excludes).ToList();
            if (!availableIds.Any()) return null;

            var random = new Random();
            var selectedId = availableIds[random.Next(availableIds.Count)];

            return await _context.RestaurantAdBanners
                .Include(b => b.Restaurant)
                .FirstOrDefaultAsync(b => b.Id == selectedId);
        }

        /* ============================================================
           🔹 Atomic impression counter
        ============================================================ */
        public async Task<bool> IncrementBannerImpressionAsync(int bannerId)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [RestaurantAdBanners]
                SET [ConsumedViews] = [ConsumedViews] + 1
                WHERE [Id] = {bannerId}
                  AND [IsPaused] = 0
                  AND [StartDate] <= GETUTCDATE()
                  AND [EndDate]   >= GETUTCDATE()
                  AND ([PurchasedViews] = 0 OR [ConsumedViews] < [PurchasedViews]);
            ");
            return rows > 0;
        }

        /* ============================================================
           🔹 Restaurants ordered by user (no caching - user specific)
        ============================================================ */
        public async Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return new();

            var latestByRestaurant = await _context.Orders
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.RestaurantId)
                .Select(g => new { RestaurantId = g.Key, LastOrderAt = g.Max(o => o.CreatedAt) })
                .OrderByDescending(x => x.LastOrderAt)
                .ToListAsync();

            if (latestByRestaurant.Count == 0) return new();

            var ids = latestByRestaurant.Select(x => x.RestaurantId).ToList();

            var restaurants = await _context.Restaurants
                .Where(r => ids.Contains(r.Id))
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .ToListAsync();

            var orderMap = latestByRestaurant.ToDictionary(x => x.RestaurantId, x => x.LastOrderAt);
            return restaurants
                .OrderByDescending(r => orderMap.ContainsKey(r.Id) ? orderMap[r.Id] : DateTime.MinValue)
                .ToList();
        }

        /* ============================================================
           🔹 Restaurant Page (Banner + Slug + Validation)
        ============================================================ */
        public async Task<Restaurant?> GetRestaurantBannerBySlugAsync(string slug)
        {
            string cacheKey = $"RestaurantBanner:{slug}";
            if (_cache.TryGetValue(cacheKey, out Restaurant cached))
                return cached;

            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive && r.IsApproved);

            if (restaurant != null)
                _cache.Set(cacheKey, restaurant, TimeSpan.FromMinutes(10));

            return restaurant;
        }

        /* ============================================================
           🔹 Slug checks
        ============================================================ */
        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Restaurants.AnyAsync(r => r.Slug == slug);
        }

        /* ============================================================
           🔹 RestaurantId lookup by OwnerUserId
        ============================================================ */
        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            string cacheKey = $"RestaurantIdByUser:{userId}";
            if (_cache.TryGetValue(cacheKey, out int cachedId))
                return cachedId;

            var id = await _context.Restaurants
                .Where(r => r.OwnerUserId == userId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            _cache.Set(cacheKey, id, TimeSpan.FromMinutes(30));
            return id;
        }

        /* ============================================================
           🔄 Cache invalidation methods
        ============================================================ */
        public void InvalidateFeaturedRestaurants() => _cache.Remove("FeaturedRestaurants");
        public void InvalidateRandomRestaurants() => _cache.Remove("RandomRestaurants");
        public void InvalidateRestaurantBanner(string slug) => _cache.Remove($"RestaurantBanner:{slug}");
        public void InvalidateRestaurantIdByUser(string userId) => _cache.Remove($"RestaurantIdByUser:{userId}");
        public void InvalidateBannerIds() => _cache.Remove("LiveBannerIds");

        // admin panel => restaurant management tab
        public async Task<List<Restaurant>> GetRestaurantsListForAdminAsync(bool? approvedStatus = null)
        {
            var query = _context.Restaurants
                .Include(r => r.OwnerUser)
                .AsQueryable();

            if (approvedStatus != null)
                query = query.Where(r => r.IsApproved == approvedStatus);

            return await query
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }
        public async Task<Restaurant?> GetRestaurantDetailsForAdminAsync(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.OwnerUser)
                .Include(r => r.RestaurantCategory)
                .FirstOrDefaultAsync(r => r.Id == id);
            return restaurant;
        }

        
        // restaurant profile
        public async Task<Restaurant?> GetRestaurantProfileAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.OwnerUser)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            return restaurant;

        }



    }
}