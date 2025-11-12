using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RandomRestaurantCardService : IRandomRestaurantCardService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RandomRestaurantCardService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8)
        {
            Console.WriteLine("➡️ Fetching random restaurants...");
            var restaurants = await _restaurantRepository.GetRandomActiveApprovedWithDetailsAsync(count);
            System.Diagnostics.Debug.WriteLine("✅ Got " + restaurants.Count + " restaurants from repository");

            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            return restaurants.Select(r =>
            {
                double avgRating = r.Ratings?.Any() == true
                    ? Math.Round(r.Ratings.Average(rt => rt.Score), 1)
                    : 0;

                int voters = r.Ratings?.Count ?? 0;

                int? discountPercent = r.Discounts?
                    .Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc)
                    .Select(d => (int?)d.Percent)
                    .DefaultIfEmpty(null)
                    .Max();

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
        }
    }
}
