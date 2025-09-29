using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantBannerService : IRestaurantBannerService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantBannerService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantBannerDto?> GetRestaurantBannerBySlugAsync(string slug)
        {
            var restaurant = await _restaurantRepository.GetBySlugWithRatingsAsync(slug);

            if (restaurant == null)
                return null;

            return new RestaurantBannerDto
            {
                Name = restaurant.Name,
                BannerImageUrl = restaurant.BannerImageUrl,
                AverageRating = restaurant.Ratings.Any()
                    ? restaurant.Ratings.Average(r => r.Score)
                    : 0,
                VotersCount = restaurant.Ratings.Count
            };
        }

    }
}
