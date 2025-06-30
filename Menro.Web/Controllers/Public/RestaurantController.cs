using Menro.Application.Services.Interfaces;
using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class RestaurantController : ControllerBase
    {
        private readonly ILogger<RestaurantController> _logger;
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(ILogger<RestaurantController> logger, IRestaurantService restaurantService)
        {
            _logger = logger;
            _restaurantService = restaurantService;
        }

        //Featured Restaurants Carousel
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedRestaurants()
        {
            var featured = await _restaurantService.GetFeaturedRestaurantsAsync();
            return Ok(featured);
        }
    }
}