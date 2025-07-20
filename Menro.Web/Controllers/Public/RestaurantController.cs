using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IUserRecentOrderCardService _userRecentOrderCardService;
        private readonly IRestaurantAdBannerService _restaurantAdBannerService;
        private readonly IRestaurantShopBannerService _restaurantShopBannerService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IUserRecentOrderCardService userRecentOrderCardService,
            IRestaurantShopBannerService restaurantShopBannerService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
            _userRecentOrderCardService = userRecentOrderCardService;
            _restaurantShopBannerService = restaurantShopBannerService;
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedRestaurants()
        {
            var featuredRestaurants = await _featuredRestaurantService.GetFeaturedRestaurantsAsync();
            return Ok(featuredRestaurants);
        }

        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRandomRestaurants()
        {
            var result = await _randomRestaurantCardService.GetRandomRestaurantCardsAsync();
            return Ok(result);
        }

        [HttpGet("ad-banner")]
        public async Task<ActionResult<RestaurantAdBannerDto>> GetAdBanner()
        {
            var banner = await _restaurantAdBannerService.GetActiveAdBannerAsync();
            if (banner == null)
                return NoContent();

            return Ok(banner);
        }

        // ✅ New Shop Page - Get restaurant info for banner by slug
        [HttpGet("banner/{slug}")]
        public async Task<ActionResult<RestaurantShopBannerDto>> GetBannerBySlug(string slug)
        {
            var dto = await _restaurantShopBannerService.GetShopBannerAsync(slug);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }
    }
}
