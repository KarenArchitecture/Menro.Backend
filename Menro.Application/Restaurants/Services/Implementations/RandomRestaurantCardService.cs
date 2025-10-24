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
                return cachedList.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
            }

            var restaurants = await _restaurantRepository.GetRandomActiveApprovedWithDetailsAsync(count * 2);
            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            var dtoList = restaurants.Select(r =>
            {
                // ✅ Compute rating from Restaurant.Ratings (new system)
                double avgRating = 0.0;
                int voters = 0;

                if (r.Ratings != null && r.Ratings.Any())
                {
                    avgRating = Math.Round(r.Ratings.Average(rt => rt.Score), 1);
                    voters = r.Ratings.Count;
                }

                // ✅ Active discounts
                int? discountPercent = null;
                var activeDiscounts = r.Discounts?
                    .Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc)
                    .ToList() ?? new List<RestaurantDiscount>();

                if (activeDiscounts.Count > 0)
                    discountPercent = activeDiscounts.Max(d => d.Percent);

                // ✅ Open/close status
                bool isOpen = r.OpenTime <= r.CloseTime
                    ? nowTime >= r.OpenTime && nowTime < r.CloseTime
                    : nowTime >= r.OpenTime || nowTime < r.CloseTime;

                return new RestaurantCardDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
                    BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                        ? "/img/res-cards.png"
                        : r.BannerImageUrl,
                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? "/img/res-slider.png"
                        : r.LogoImageUrl,
                    Rating = avgRating,
                    Voters = voters,
                    Discount = discountPercent,
                    OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                    CloseTime = r.CloseTime.ToString(@"hh\:mm"),
                    IsOpen = isOpen,
                    Slug = r.Slug
                };
            }).ToList();

            _cache.Set(CacheKey, dtoList, CacheDuration);

            return dtoList.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        // 🔄 Optional cache invalidation
        public void InvalidateRandomRestaurantsCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}
