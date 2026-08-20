using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.FoodCategories.Services.Interfaces;
using Menro.Application.Features.FoodCategories.DTOs;
using Menro.Application.Common.SD;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Application.Features.Search.DTOs;
using Menro.Application.Features.Users.Services.Interfaces;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Route("api/public/restaurant")]
    public class PublicRestaurantController : ApiControllerBase
    {
        #region DI

        private readonly IRestaurantService _restaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IRestaurantBrowseService _restaurantBrowseService;

        private readonly IRestaurantBannerService _restaurantBannerService;
        private readonly IRestaurantMenuService _menuService;
        private readonly IRestaurantPageFoodCategoryService _restaurantPageFoodCategoryService;

        public PublicRestaurantController(
            IRestaurantService restaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IRestaurantBrowseService restaurantBrowseService,
            IRestaurantBannerService restaurantBannerService,
            IRestaurantPageFoodCategoryService restaurantPageFoodCategoryService,
            IRestaurantMenuService menuService)
        {
            _restaurantService = restaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _restaurantBrowseService = restaurantBrowseService;

            _restaurantBannerService = restaurantBannerService;
            _menuService = menuService;
            _restaurantPageFoodCategoryService = restaurantPageFoodCategoryService;
        }

        #endregion

        #region Home Page Endpoints

        // GET: /api/public/restaurant/random
        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRandomRestaurants()
        {
            var result = await _randomRestaurantCardService.GetRandomRestaurantCardsAsync();
            return Ok(result);
        }

        #endregion

        #region Show All Page - Restaurants (Browse)

        // GET: /api/public/restaurant?take=20&cursor=123
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<RestaurantCardDto>>> GetRestaurants(
            [FromQuery] int take = 20,
            [FromQuery] int? cursor = null)
        {
            var result = await _restaurantBrowseService.GetRestaurantsPageAsync(take, cursor);
            return Ok(result);
        }

        #endregion

        #region Registration & Global Categories

        // POST: /api/public/restaurant/register
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult> RestaurantRegister([FromBody] RegisterRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerUserId))
                return Unauthorized("کاربر شناسایی نشد.");

            var (success, error) = await _restaurantService.AddRestaurantAsync(dto, ownerUserId);
            if (!success)
                return BadRequest(error ?? "ثبت رستوران با خطا مواجه شد.");
            return Ok("رستوران با موفقیت ثبت شد.");
        }

        // GET /api/public/restaurant/my-status
        [HttpGet("my-status")]
        [Authorize]
        public async Task<IActionResult> GetMyRestaurantStatus()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _restaurantService.GetOwnerRestaurantStatusAsync(userId);
            if (result == null) return NotFound(); // کاربر هیچ رستورانی ثبت نکرده
            return Ok(result);
        }

        // GET: /api/public/restaurant/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetRestaurantCategories()
        {
            var categories = await _restaurantService.GetRestaurantCategoriesAsync();
            return Ok(categories);
        }

        #endregion

        #region Restaurant Page Endpoints

        // GET: /api/public/restaurant/{slug}/banner
        [HttpGet("{slug}/banner")]
        public async Task<ActionResult<RestaurantBannerDto?>> GetBanner(string slug)
        {
            var banner = await _restaurantBannerService.GetBannerBySlugAsync(slug);
            if (banner == null)
                return NotFound();

            return Ok(banner);
        }

        [HttpGet("{slug}/categories")]
        public async Task<ActionResult<List<RestaurantFoodCategoryDto>>> GetRestaurantCategoriesBySlug(
            string slug,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest(new { message = "Slug cannot be empty." });

            var categories = await _restaurantPageFoodCategoryService.GetRestaurantCategoriesAsync(slug, ct);

            if (categories == null)
                return NotFound(new { message = "رستوران یافت نشد." });

            return Ok(categories); // خالی هم باشه، 200 با [] برمی‌گرده
        }

        [HttpGet("{slug}/menu")]
        public async Task<ActionResult<List<RestaurantMenuDto>>> GetRestaurantMenuBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest(new { message = "Slug cannot be empty." });

            var menu = await _menuService.GetMenuBySlugAsync(slug);

            if (menu == null)
                return NotFound(new { message = "رستوران یافت نشد." });

            return Ok(menu);
        }
        #endregion
    }
}