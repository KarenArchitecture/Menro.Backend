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
using Menro.Application.Restaurants.Services.Implementations;
using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Implementations;

namespace Menro.Web.Controllers.Public
{
    /// <summary>
    /// Provides public-facing endpoints for restaurants, including featured lists,
    /// random selections, ad banners, registration, categories, and detailed
    /// restaurant pages (banner + menu).
    /// </summary>
    [ApiController]
    [Route("api/public/[controller]")]
    public class RestaurantController : ControllerBase
    {
        #region Fields & Constructor

        private readonly IRestaurantService _restaurantService;
        private readonly IFeaturedRestaurantService _featuredRestaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IUserRecentOrderCardService _userRecentOrderCardService;
        private readonly IRestaurantAdBannerService _restaurantAdBannerService;
        private readonly IRestaurantBannerService _restaurantBannerService;
        private readonly IRestaurantMenuService _restaurantMenuService;
        private readonly IAuthService _authService;
        private readonly IRestaurantBannerService _bannerService;
        private readonly IMenuListService _menuListService;
        private readonly IMenuItemService _menuItemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestaurantController"/>.
        /// </summary>
        public RestaurantController(
            IRestaurantService restaurantService,
            IFeaturedRestaurantService featuredRestaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IUserRecentOrderCardService userRecentOrderCardService,
            IRestaurantAdBannerService restaurantAdBannerService,
            IRestaurantBannerService restaurantBannerService,
            IRestaurantMenuService restaurantMenuService,
            IAuthService authService,
            IMenuListService menuListService,
            IMenuItemService menuItemService)
        {
            _restaurantService = restaurantService;
            _featuredRestaurantService = featuredRestaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _userRecentOrderCardService = userRecentOrderCardService;
            _restaurantAdBannerService = restaurantAdBannerService;
            _restaurantBannerService = restaurantBannerService;
            _restaurantMenuService = restaurantMenuService;
            _authService = authService;
            _menuListService = menuListService;
            _menuItemService = menuItemService;
        }

        #endregion

        #region Home Page Endpoints

        /// <summary>
        /// Retrieves the list of featured restaurants (highlighted on the home page).
        /// </summary>
        /// <returns>A list of featured restaurants.</returns>
        /// <response code="200">Successfully returned featured restaurants.</response>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedRestaurants()
        {
            var featuredRestaurants = await _featuredRestaurantService.GetFeaturedRestaurantsAsync();
            return Ok(featuredRestaurants);
        }

        /// <summary>
        /// Retrieves a random set of restaurants for variety on the home page.
        /// </summary>
        /// <returns>A random collection of restaurant cards.</returns>
        /// <response code="200">Successfully returned random restaurants.</response>
        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRandomRestaurants()
        {
            var result = await _randomRestaurantCardService.GetRandomRestaurantCardsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a random restaurant advertisement banner, optionally excluding some IDs.
        /// </summary>
        /// <param name="exclude">Comma-separated list of ad IDs to exclude.</param>
        /// <returns>A random ad banner, or 204 if none are available.</returns>
        /// <response code="200">Random ad banner found.</response>
        /// <response code="204">No ad banners available after exclusions.</response>
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

        /// <summary>
        /// Tracks an impression (view) for a specific advertisement banner.
        /// </summary>
        /// <param name="id">The ad banner ID.</param>
        /// <returns>No content on success, 404 if banner not found.</returns>
        /// <response code="204">Impression successfully tracked.</response>
        /// <response code="404">Ad banner not found.</response>
        [HttpPost("ad-banner/{id}/impression")]
        public async Task<IActionResult> TrackAdImpression(int id)
        {
            var ok = await _restaurantAdBannerService.AddImpressionAsync(id);
            return ok ? NoContent() : NotFound();
        }

        #endregion

        #region Registration & Categories

        /// <summary>
        /// Registers a new restaurant for the currently authenticated user.
        /// </summary>
        /// <param name="dto">The restaurant registration data.</param>
        /// <returns>Status of the registration attempt.</returns>
        /// <response code="200">Restaurant registered successfully.</response>
        /// <response code="400">Invalid input or registration failed.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult> RestaurantRegister([FromBody] RegisterRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ownerUserId))
                return Unauthorized("کاربر شناسایی نشد.");

            var success = await _restaurantService.AddRestaurantAsync(dto, ownerUserId);

            if (!success)
                return BadRequest("ثبت رستوران با خطا مواجه شد.");

            await _authService.AddRoleToUserAsync(ownerUserId, SD.Role_Owner);
            return Ok("رستوران با موفقیت ثبت شد.");
        }

        /// <summary>
        /// Retrieves all restaurant categories.
        /// </summary>
        /// <returns>A list of restaurant categories.</returns>
        /// <response code="200">Successfully returned restaurant categories.</response>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _restaurantService.GetRestaurantCategoriesAsync();
            return Ok(categories);
        }

        #endregion

        #region Restaurant Page Endpoints
        // ===== BANNER =====
        [HttpGet("banner/{slug}")]
        public async Task<ActionResult<RestaurantBannerDto?>> GetBanner(string slug)
        {
            var banner = await _restaurantBannerService.GetBannerBySlugAsync(slug);
            if (banner == null)
                return NotFound();

            return Ok(banner);
        }

        /// <summary>
        /// Get full menu list of a restaurant (optional category filter)
        /// GET: /api/public/restaurant/menu/{restaurantId}?globalCategoryId=1&customCategoryId=2
        /// </summary>
        [HttpGet("menu/{slug}")]
        public async Task<ActionResult<List<MenuListFoodDto>>> GetRestaurantMenuBySlug(
            string slug,
            [FromQuery] int? globalCategoryId = null,
            [FromQuery] int? customCategoryId = null)
        {
            var foods = await _menuListService.GetMenuListBySlugAsync(slug, globalCategoryId, customCategoryId);
            return Ok(foods);
        }

        /// <summary>
        /// Get detailed info of a single food item (with variants & addons)
        /// GET: /api/public/restaurant/{foodId}/details
        /// </summary>
        [HttpGet("{foodId}/details")]
        public async Task<ActionResult<MenuFoodDetailDto>> GetRestaurantFoodDetail(int foodId)
        {
            var foodDetail = await _menuItemService.GetMenuItemDetailAsync(foodId);
            if (foodDetail == null) return NotFound();
            return Ok(foodDetail);
        }
        #endregion



    }
}
