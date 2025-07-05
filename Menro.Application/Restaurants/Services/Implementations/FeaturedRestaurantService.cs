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
    public class FeaturedRestaurantService: IFeaturedRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public FeaturedRestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<IEnumerable<FeaturedRestaurantDto>> GetFeaturedRestaurantsAsync()
        {
            var featuredRestaurants = await _restaurantRepository.GetFeaturedRestaurantsAsync();

            return featuredRestaurants.Select(r => new FeaturedRestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                CarouselImageUrl = r.CarouselImageUrl
            });
        }
    }
}
