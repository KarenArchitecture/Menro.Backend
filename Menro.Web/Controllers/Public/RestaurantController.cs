using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;
using Menro.Application.Features.Identity.Services;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.Services.Interfaces;

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
        private readonly IRestaurantShopBannerService _restaurantShopBannerService;
        private readonly IRestaurantMenuService _restaurantMenuService;
        private readonly IAuthService _authService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IUserRecentOrderCardService userRecentOrderCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IRestaurantShopBannerService restaurantShopBannerService,
            IRestaurantMenuService restaurantMenuService,
            IAuthService authService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _userRecentOrderCardService = userRecentOrderCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
            _restaurantShopBannerService = restaurantShopBannerService;
            _restaurantMenuService = restaurantMenuService;
            _authService = authService;
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

        // GET api/public/restaurant/ad-banner/random?exclude=1,3,5
        [HttpGet("ad-banner/random")]
        public async Task<ActionResult<RestaurantAdBannerDto>> GetRandomAdBanner([FromQuery] string? exclude)
        {
            var excludeIds = string.IsNullOrWhiteSpace(exclude)
            ? new List<int>()
            : exclude.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(s => int.TryParse(s, out var x) ? (int?)x : null)
                     .Where(x => x.HasValue)
                     .Select(x => x.Value)
                     .Distinct()
                     .ToList();

            var dto = await _restaurantAdBannerService.GetRandomAdBannerAsync(excludeIds);
            if (dto == null) return NoContent();
            return Ok(dto);
        }

        // POST api/public/restaurant/ad-banner/{id}/impression
        [HttpPost("ad-banner/{id}/impression")]
        public async Task<IActionResult> TrackAdImpression(int id)
        {
            var ok = await _restaurantAdBannerService.AddImpressionAsync(id);
            return ok ? NoContent() : NotFound();
        }



        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult> RestaurantRegister([FromBody] RegisterRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            // گرفتن شناسه کاربر از توکن (claims) مناسب برای کلاس ها و سرویس هایی که کنترلر نیستن و User رو در دسترس ندارن
            //var ownerUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ownerUserId))
                return Unauthorized("کاربر شناسایی نشد.");

            var success = await _restaurantService.AddRestaurantAsync(dto, ownerUserId);

            if (!success)
                return BadRequest("ثبت رستوران با خطا مواجه شد.");
            await _authService.AddRoleToUserAsync(ownerUserId, SD.Role_Owner);
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
    }
}
