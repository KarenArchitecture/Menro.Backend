using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;


namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRestaurantCardService _restaurantCardService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRestaurantCardService restaurantCardService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _restaurantCardService = restaurantCardService;
        }

        // GET: api/public/restaurant/featured
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedRestaurants()
        {
            var featuredRestaurants = await _featuredRestaurantService.GetFeaturedRestaurantsAsync();
            return Ok(featuredRestaurants);
        }

        // GET: api/public/restaurant/random
        //[HttpGet("random")]
        //public async Task<IActionResult> GetRandomRestaurants()
        //{
        //    var randomRestaurants = await _restaurantService.GetRandomRestaurantsAsync();
        //    return Ok(randomRestaurants);
        //}

        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRandomRestaurants()
        {
            var result = await _restaurantCardService.GetRandomRestaurantCardsAsync();
            return Ok(result);
        }
        // You can add more endpoints here as needed...
    }
}