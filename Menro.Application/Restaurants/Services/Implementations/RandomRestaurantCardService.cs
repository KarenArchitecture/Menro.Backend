using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //public async Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8)
        //{
        //    var restaurants = await _restaurantRepository.GetAllActiveApprovedWithDetailsAsync();

        //    // Local time-of-day for open/closed comparison
        //    var nowTime = DateTime.Now.TimeOfDay;     // use local server time
        //    var nowUtc = DateTime.UtcNow;            // for active discount windows

        //    var dtoList = restaurants.Select(r =>
        //    {
        //        var avgRating = r.Ratings.Any() ? r.Ratings.Average(rt => rt.Score) : 0;
        //        var voters = r.Ratings.Count;

        //        // ✅ Max active discount among all item/restaurant-wide discounts
        //        int? discountPercent = null;
        //        var activeDiscounts = r.Discounts
        //            .Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc)
        //            .ToList();
        //        if (activeDiscounts.Count > 0)
        //            discountPercent = activeDiscounts.Max(d => d.Percent);

        //        // ✅ Open/Closed status (handles overnight ranges e.g., 23:00 → 09:00)
        //        var open = r.OpenTime;
        //        var close = r.CloseTime;
        //        bool isOpen;
        //        if (open <= close)
        //            isOpen = nowTime >= open && nowTime < close;     // same-day window
        //        else
        //            isOpen = nowTime >= open || nowTime < close;     // overnight window

        //        return new RestaurantCardDto
        //        {
        //            Id = r.Id,
        //            Name = r.Name,
        //            Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
        //            BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl) ? "/img/res-cards.png" : r.BannerImageUrl,
        //            LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl) ? "/img/res-slider.png" : r.LogoImageUrl,
        //            Rating = avgRating,
        //            Voters = voters,
        //            Discount = discountPercent,
        //            OpenTime = r.OpenTime.ToString(@"hh\:mm"),
        //            CloseTime = r.CloseTime.ToString(@"hh\:mm"),
        //            IsOpen = isOpen,
        //            Slug = r.Slug
        //        };
        //    })
        //    .OrderBy(_ => Guid.NewGuid())
        //    .Take(count)
        //    .ToList();

        //    return dtoList;
        //}

        public async Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8)
        {
            // ✅ Step 1: Get random subset directly from DB
            var randomRestaurants = await _restaurantRepository.GetRandomActiveApprovedWithDetailsAsync(count * 2);

            // ✅ Step 2: Process fields safely in memory
            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            var dtoList = randomRestaurants.Select(r =>
            {
                var avgRating = r.Ratings.Any() ? r.Ratings.Average(rt => rt.Score) : 0;
                var voters = r.Ratings.Count;

                int? discountPercent = null;
                var activeDiscounts = r.Discounts
                    .Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc)
                    .ToList();
                if (activeDiscounts.Count > 0)
                    discountPercent = activeDiscounts.Max(d => d.Percent);

                bool isOpen;
                if (r.OpenTime <= r.CloseTime)
                    isOpen = nowTime >= r.OpenTime && nowTime < r.CloseTime; // same-day range
                else
                    isOpen = nowTime >= r.OpenTime || nowTime < r.CloseTime; // overnight range

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
            })
            .OrderBy(_ => Guid.NewGuid()) // small extra shuffle in memory
            .Take(count)
            .ToList();

            return dtoList;
        }






    }
}
