using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantBannerService : IRestaurantBannerService
    {
        #region DI
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMediaStorageProvider _mediaStorage;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public RestaurantBannerService(
            IRestaurantRepository restaurantRepository,
            IMediaStorageProvider mediaStorage,
            IMemoryCache cache)
        {
            _restaurantRepository = restaurantRepository;
            _mediaStorage = mediaStorage;
            _cache = cache;
        }

        #endregion
        public async Task<RestaurantBannerDto?> GetBannerBySlugAsync(string slug)
        {
            string cacheKey = $"restaurant_banner_{slug}";
            if (_cache.TryGetValue(cacheKey, out RestaurantBannerDto? cached) && cached is not null)
                return cached;

            var restaurant = await _restaurantRepository.GetRestaurantBannerBySlugAsync(slug);
            if (restaurant == null)
                return null;

            var entityId = restaurant.Id.ToString();

            var dto = new RestaurantBannerDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                BannerImageUrl = string.IsNullOrWhiteSpace(restaurant.ShopBannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantShopBanner, restaurant.ShopBannerImageUrl, entityId, MediaVariant.Resized),
                AverageRating = restaurant.Ratings?.Any() == true
                    ? Math.Round(restaurant.Ratings.Average(r => r.Score), 1)
                    : 0.0,
                VotersCount = restaurant.Ratings?.Count ?? 0,
                TableCount = restaurant.Tables.Count
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