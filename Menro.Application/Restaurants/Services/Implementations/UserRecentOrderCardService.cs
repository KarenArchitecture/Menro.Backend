using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class UserRecentOrderCardService : IUserRecentOrderCardService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public UserRecentOrderCardService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<List<RestaurantCardDto>> GetRecentOrderedRestaurantCardsAsync(string userId, int count = 8)
        {
            var restaurants = await _restaurantRepository.GetRestaurantsOrderedByUserAsync(userId);
            var now = DateTime.UtcNow;

            return restaurants
                .Take(count) // ✅ limit to 'count' recent restaurants
                .Select(r =>
                {
                    var avgRating = r.Ratings.Any() ? r.Ratings.Average(rt => rt.Score) : 0;
                    var voters = r.Ratings.Count;

                    int? discountPercent = null;
                    var activeDiscount = r.Discounts.FirstOrDefault(d => d.StartDate <= now && d.EndDate >= now);
                    if (activeDiscount != null)
                        discountPercent = activeDiscount.Percent;

                    return new RestaurantCardDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
                        BannerImageUrl = r.BannerImageUrl,
                        Rating = avgRating,
                        Voters = voters,
                        Discount = discountPercent,
                        OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                        CloseTime = r.CloseTime.ToString(@"hh\:mm")
                    };
                }).ToList();
        }
    }
}
