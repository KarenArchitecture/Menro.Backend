using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;


namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRestaurantCardService _restaurantCardService;
        private readonly IRestaurantAdBannerService _restaurantAdBannerService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRestaurantCardService restaurantCardService,
            IRestaurantAdBannerService restaurantAdBannerService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _restaurantCardService = restaurantCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
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
            var result = await _restaurantCardService.GetRandomRestaurantCardsAsync();
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
    }
}