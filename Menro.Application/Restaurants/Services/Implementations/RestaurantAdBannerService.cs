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
    public class RestaurantAdBannerService : IRestaurantAdBannerService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantAdBannerService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantAdBannerDto> GetActiveAdBannerAsync()
        {
            var banner = await _restaurantRepository.GetActiveAdBannerAsync();

            if (banner == null)
                return null;

            return new RestaurantAdBannerDto
            {
                RestaurantId = banner.RestaurantId,
                RestaurantName = banner.Restaurant.Name,
                ImageUrl = banner.ImageUrl
            };
        }
    }

}
