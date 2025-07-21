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
    public class RestaurantShopBannerService : IRestaurantShopBannerService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantShopBannerService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantShopBannerDto?> GetShopBannerAsync(string slug)
        {
            var restaurant = await _restaurantRepository.GetBySlugWithCategoryAsync(slug);
            if (restaurant == null) return null;

            return new RestaurantShopBannerDto
            {
                Name = restaurant.Name,
                CategoryName = restaurant.RestaurantCategory.Name,
                BannerImageUrl = restaurant.BannerImageUrl
            };
        }
    }
}
