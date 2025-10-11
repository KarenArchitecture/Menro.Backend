using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RandomRestaurantCardService : IRandomRestaurantCardService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private const string CacheKey = "random_restaurants_cache";

        public RandomRestaurantCardService(IRestaurantRepository restaurantRepository, IMemoryCache cache)
        {
            _restaurantRepository = restaurantRepository;
            _cache = cache;
        }

        public async Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8)
        {
            if (_cache.TryGetValue(CacheKey, out List<RestaurantCardDto> cachedList))
            {
                // ✅ Pick random items from cached list
                return cachedList.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
            }

            // Fetch from DB if cache is empty
            var randomRestaurants = await _restaurantRepository.GetRandomActiveApprovedWithDetailsAsync(count * 2);

            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            var dtoList = randomRestaurants.Select(r =>
            {
                var avgRating = r.Ratings?.Any() == true ? r.Ratings.Average(rt => rt.Score) : 0;
                var voters = r.Ratings?.Count ?? 0;

                int? discountPercent = null;
                var activeDiscounts = r.Discounts?.Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc).ToList()
                      ?? new List<RestaurantDiscount>();

                if (activeDiscounts.Count > 0)
                    discountPercent = activeDiscounts.Max(d => d.Percent);

                bool isOpen;
                if (r.OpenTime <= r.CloseTime)
                    isOpen = nowTime >= r.OpenTime && nowTime < r.CloseTime;
                else
                    isOpen = nowTime >= r.OpenTime || nowTime < r.CloseTime;

                return new RestaurantCardDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
                    BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl) ? "/img/res-cards.png" : r.BannerImageUrl,
                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl) ? "/img/res-slider.png" : r.LogoImageUrl,
                    Rating = avgRating,
                    Voters = voters,
                    Discount = discountPercent,
                    OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                    CloseTime = r.CloseTime.ToString(@"hh\:mm"),
                    IsOpen = isOpen,
                    Slug = r.Slug
                };
            }).ToList();

            // Store full list in cache
            _cache.Set(CacheKey, dtoList, CacheDuration);

            // Return randomized subset
            return dtoList.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        // 🔄 Optional cache invalidation
        public void InvalidateRandomRestaurantsCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}
