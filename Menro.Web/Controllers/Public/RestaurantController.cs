using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class RestaurantController : ControllerBase
    {
        #region
        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IUserRecentOrderCardService _userRecentOrderCardService;
        private readonly IRestaurantAdBannerService _restaurantAdBannerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRestaurantShopBannerService _restaurantShopBannerService;
        private readonly IRestaurantMenuService _restaurantMenuService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IUserRecentOrderCardService userRecentOrderCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IRestaurantShopBannerService restaurantShopBannerService,
            IRestaurantMenuService restaurantMenuService,
            IHttpContextAccessor httpContextAccessor)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _userRecentOrderCardService = userRecentOrderCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
            _restaurantShopBannerService = restaurantShopBannerService;
            _httpContextAccessor = httpContextAccessor;
            _restaurantMenuService = restaurantMenuService;
        }
        #endregion
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

        // New Shop Page - Get restaurant info for banner by slug
        [HttpGet("{slug}/banner")]
        public async Task<ActionResult<RestaurantShopBannerDto>> GetBannerBySlug(string slug)
        {
            var dto = await _restaurantShopBannerService.GetShopBannerAsync(slug);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }
        /* Restaurant Public Page
        Returns the full menu of a restaurant, grouped by category.
        GET: api/public/restaurant/{slug}/menu
        */
        [HttpGet("{slug}/menu")]
        public async Task<ActionResult<List<RestaurantMenuDto>>> GetMenu(string slug)
        {
            var sections = await _restaurantMenuService.GetRestaurantMenuBySlugAsync(slug);

            if (sections == null || sections.Count == 0)
                return NotFound(); // 404 if restaurant not found / no foods

            return Ok(sections); // 200 + JSON payload
        }

        [HttpGet("restaurant-id")]
        public async Task<IActionResult> GetRestaurantId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // یا روش JWT خودت
            int? restaurantId = await _restaurantService.GetRestaurantIdByUserIdAsync(userId);
            return Ok(new { restaurantId });
        }
    }
}
