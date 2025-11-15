using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantBannerService : IRestaurantBannerService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public RestaurantBannerService(IRestaurantRepository restaurantRepository, IMemoryCache cache)
        {
            _restaurantRepository = restaurantRepository;
            _cache = cache;
        }

        public async Task<RestaurantBannerDto?> GetBannerBySlugAsync(string slug)
        {
            string cacheKey = $"restaurant_banner_{slug}";

            if (_cache.TryGetValue(cacheKey, out RestaurantBannerDto cached))
                return cached;

            var restaurant = await _restaurantRepository.GetRestaurantBannerBySlugAsync(slug);
            if (restaurant == null) return null;

            var dto = new RestaurantBannerDto
            {
                Name = restaurant.Name,
                // ✅ now use new field for shop page banner
                BannerImageUrl = string.IsNullOrWhiteSpace(restaurant.ShopBannerImageUrl)
                    ? "/img/ad-banner-2.png"
                    : restaurant.ShopBannerImageUrl,
                AverageRating = restaurant.Ratings?.Any() == true
                    ? Math.Round(restaurant.Ratings.Average(r => r.Score), 1)
                    : 0.0,
                VotersCount = restaurant.Ratings?.Count ?? 0
            };

            _cache.Set(cacheKey, dto, CacheDuration);
            return dto;
        }

        public void InvalidateCache(string slug)
        {
            string cacheKey = $"restaurant_banner_{slug}";
            _cache.Remove(cacheKey);
        }
    }
}
