using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.Services.Implementations;
using Menro.Application.Restaurants.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;


namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantController : ControllerBase
    {

        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IUserRecentOrderCardService _userRecentOrderCardService;
        private readonly IRestaurantAdBannerService _restaurantAdBannerService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRestaurantCardService restaurantCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IHttpContextAccessor httpContextAccessor)
            IRandomRestaurantCardService randomRestaurantCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IUserRecentOrderCardService userRecentOrderCardService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
            _httpContextAccessor = httpContextAccessor;
            _userRecentOrderCardService = userRecentOrderCardService;
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
        
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult> RestaurantRegister([FromBody] RegisterRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            // گرفتن شناسه کاربر از توکن (claims)
            var ownerUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(ownerUserId))
                return Unauthorized("کاربر شناسایی نشد.");

            var success = await _restaurantService.AddRestaurantAsync(dto, ownerUserId);

            if (!success)
                return BadRequest("ثبت رستوران با خطا مواجه شد.");

            return Ok("رستوران با موفقیت ثبت شد.");
        }
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _restaurantService.GetRestaurantCategoriesAsync();
            return Ok(categories); // JSON اتوماتیک ارسال میشه
        }
    }
}