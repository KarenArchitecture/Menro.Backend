using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        //Home Page - Random Restaurants Cards
        public async Task<List<Restaurant>> GetAllActiveApprovedWithDetailsAsync()
        {
            return await _context.Restaurants
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .Where(r => r.IsActive && r.IsApproved)
                .ToListAsync();
        }

        //Home Page - Featured Restaurant Banner
        // Home Page - Random eligible Ad Banner (exclude already-served ids)
        public async Task<RestaurantAdBanner?> GetRandomLiveAdBannerAsync(IEnumerable<int> excludeIds)
        {
            var now = DateTime.UtcNow;
            var excludes = excludeIds?.ToList() ?? new List<int>();

            var query = _context.RestaurantAdBanners
                .Include(b => b.Restaurant)
                .Where(b =>
                    b.StartDate <= now &&
                    b.EndDate >= now &&
                    !b.IsPaused &&
                    (b.PurchasedViews == 0 || b.ConsumedViews < b.PurchasedViews) &&
                    b.Restaurant.IsActive &&
                    b.Restaurant.IsApproved);

            if (excludes.Count > 0)
                query = query.Where(b => !excludes.Contains(b.Id));

            // random pick
            return await query
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
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
        public async Task<Restaurant?> GetBySlugWithCategoryAsync(string slug)
        {
            return await _context.Restaurants
                .Include(r => r.RestaurantCategory)
                .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive && r.IsApproved);
        }

        //Shop Page - Preventing Save For an Existing Slug
        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Restaurants.AnyAsync(r => r.Slug == slug);
        }
        public async Task<int?> GetRestaurantIdByUserIdAsync(string userId)
        {
            return await _context.Restaurants
                .Where(r => r.OwnerUserId == userId)
                .Select(r => (int?)r.Id)
                .FirstOrDefaultAsync();
        }

    }
}
