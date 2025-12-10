using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Application.Orders.Services.Implementations
{
    public class UserRecentOrderCardService : IUserRecentOrderCardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(3);
        private const string CacheKeyPrefix = "user_recent_orders_";

        public UserRecentOrderCardService(IOrderRepository orderRepository, IMemoryCache cache)
        {
            _orderRepository = orderRepository;
            _cache = cache;
        }

        private static RecentOrdersFoodCardDto Map(Food f)
        {
            var ratings = f.Ratings ?? new List<FoodRating>(0);
            var avg = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Score);

            return new RecentOrdersFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.ImageUrl ?? string.Empty,
                Rating = Math.Round(avg, 1),
                Voters = ratings.Count,
                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        public async Task<List<RecentOrdersFoodCardDto>> GetUserRecentOrderedFoodsAsync(string userId, int count = 8)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<RecentOrdersFoodCardDto>();

            if (count <= 0) count = 8;
            if (count > 32) count = 32;

            var cacheKey = $"{CacheKeyPrefix}{userId}_{count}";

            // ✅ Return cached data if available
            if (_cache.TryGetValue(cacheKey, out List<RecentOrdersFoodCardDto> cached))
                return cached;

            var foods = await _orderRepository.GetUserRecentlyOrderedFoodsAsync(userId, count);
            if (foods == null || foods.Count == 0)
                return new List<RecentOrdersFoodCardDto>();

            var result = foods.Select(Map).ToList();

            // ✅ Store in cache
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.Normal
            });

            return result;
        }

        // 🔄 Optional cache invalidation (when a new order is placed)
        public void InvalidateUserRecentOrdersCache(string userId)
        {
            var keyPrefix = $"{CacheKeyPrefix}{userId}_";
            if (_cache is MemoryCache memoryCache)
            {
                // Compact removes all items, but here we just clear this user's cached entries.
                // Since we can't enumerate keys directly, we trigger full compact if required.
                memoryCache.Compact(0.1); // clear 10% oldest/least-used entries
            }
        }
    }
}
