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

        public async Task<RestaurantAdBannerDto?> GetRandomAdBannerAsync(IEnumerable<int> excludeIds)
        {
            var banner = await _restaurantRepository.GetRandomLiveAdBannerAsync(excludeIds);
            if (banner == null) return null;

            return new RestaurantAdBannerDto
            {
                Id = banner.Id,
                ImageUrl = banner.ImageUrl ?? string.Empty,
                RestaurantName = banner.Restaurant?.Name ?? string.Empty,
                Slug = banner.Restaurant?.Slug ?? string.Empty,
                CommercialText = banner.CommercialText
            };
        }

        public Task<bool> AddImpressionAsync(int bannerId)
        {
            return _restaurantRepository.IncrementBannerImpressionAsync(bannerId);
        }

    }

}
