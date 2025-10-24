using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<string> GetRestaurantName(int restaurantId)
        {
            return await _context.Restaurants.Where(r => r.Id == restaurantId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync() ?? "منرو";
        }

        //Home Page - Featured Restaurants Carousel
        public async Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync()
        {
            return await _context.Restaurants
                .Where(r => r.IsFeatured && r.IsActive && r.IsApproved && !string.IsNullOrEmpty(r.CarouselImageUrl))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Restaurant>> GetRandomActiveApprovedWithDetailsAsync(int count)
        {
            return await _context.Restaurants
                .Where(r => r.IsActive && r.IsApproved)
                .OrderBy(r => EF.Functions.Random())                // ✅ randomize at DB level
                .Take(count)                                        // ✅ only fetch needed rows
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .Include(r => r.RestaurantCategory)
                .AsNoTracking()                                     // ✅ no EF tracking for read-only
                .ToListAsync();
        }

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public async Task<RestaurantAdBanner?> GetRandomLiveAdBannerAsync(IEnumerable<int> excludeIds)
        {
            var now = DateTime.UtcNow;
            var excludes = excludeIds?.ToList() ?? new List<int>();

            // cache only eligible IDs
            var cacheKey = "LiveBannerIds";
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

            // fetch only the selected banner
            var banner = await _context.RestaurantAdBanners
                .Include(b => b.Restaurant)
                .FirstOrDefaultAsync(b => b.Id == selectedId);

            return banner;
        }


        // Home Page - Count an impression atomically (no overshoot)
        public async Task<bool> IncrementBannerImpressionAsync(int bannerId)
        {
            // Atomic: only increment if still eligible (prevents overshoot with concurrent views)
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

        // Home Page - Latest Orders (unique restaurants, ordered by user's latest order time desc)
        public async Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return new();

            // 1) Compute the latest order timestamp per restaurant for this user
            var latestByRestaurant = await _context.Orders
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.RestaurantId)
                .Select(g => new
                {
                    RestaurantId = g.Key,
                    LastOrderAt = g.Max(o => o.CreatedAt)
                })
                .OrderByDescending(x => x.LastOrderAt)
                .ToListAsync();

            if (latestByRestaurant.Count == 0) return new();

            var ids = latestByRestaurant.Select(x => x.RestaurantId).ToList();

            // 2) Load restaurants (with details) for those ids
            var restaurants = await _context.Restaurants
                .Where(r => ids.Contains(r.Id))
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .ToListAsync();

            // 3) Preserve the "latest order" sort
            var orderMap = latestByRestaurant.ToDictionary(x => x.RestaurantId, x => x.LastOrderAt);
            var ordered = restaurants
                .OrderBy(r => orderMap.ContainsKey(r.Id) ? -orderMap[r.Id].Ticks : long.MinValue) // desc by LastOrderAt
                .ToList();

            return ordered;
        }


        //Shop Page - Restaurant Banner 
        public async Task<Restaurant?> GetRestaurantBannerBySlugAsync(string slug)
        {
            return await _context.Restaurants
                .AsNoTracking()
                .Include(r => r.Ratings) // include ratings to calculate average
                .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive && r.IsApproved);
        }

        //Shop Page - Preventing Save For an Existing Slug
        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Restaurants.AnyAsync(r => r.Slug == slug);
        }
        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            return await _context.Restaurants
                .Where(r => r.OwnerUserId == userId)
                .Select(r => (int)r.Id)
                .FirstOrDefaultAsync();
        }

    }
}
