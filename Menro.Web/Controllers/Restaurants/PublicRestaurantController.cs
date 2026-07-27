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
        private readonly IUserService _userService;
        private readonly IRestaurantMenuService _menuService;
        private readonly IRestaurantPageFoodCategoryService _restaurantPageFoodCategoryService;

        public PublicRestaurantController(
            IRestaurantService restaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IRestaurantBrowseService restaurantBrowseService,
            IRestaurantBannerService restaurantBannerService,
            IUserService userService,
            IRestaurantPageFoodCategoryService restaurantPageFoodCategoryService,
            IRestaurantMenuService menuService)
        {
            _restaurantService = restaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _restaurantBrowseService = restaurantBrowseService;

            _restaurantBannerService = restaurantBannerService;
            _userService = userService;
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

            var success = await _restaurantService.AddRestaurantAsync(dto, ownerUserId);
            if (!success)
                return BadRequest("ثبت رستوران با خطا مواجه شد.");

            await _userService.AddRoleToUserAsync(ownerUserId, SD.Role_Owner);
            return Ok("رستوران با موفقیت ثبت شد.");
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

        // GET: /api/public/restaurant/{slug}/categories
        [HttpGet("{slug}/categories")]
        public async Task<ActionResult<List<RestaurantFoodCategoryDto>>> GetRestaurantCategoriesBySlug(
            string slug,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug cannot be empty.");

            var categories = await _restaurantPageFoodCategoryService.GetRestaurantCategoriesAsync(slug, ct);

            if (categories == null || categories.Count == 0)
                return NotFound("هیچ دسته‌ای برای این رستوران یافت نشد.");

            return Ok(categories);
        }

        // GET: /api/public/restaurant/{slug}/menu
        [HttpGet("{slug}/menu")]
        public async Task<ActionResult<List<RestaurantMenuDto>>> GetRestaurantMenuBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug cannot be empty.");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var menu = await _menuService.GetMenuBySlugAsync(slug);

            sw.Stop();
            Console.WriteLine($"[RestaurantController] GetMenuBySlugAsync for slug '{slug}' took: {sw.ElapsedMilliseconds} ms");

            if (menu == null || menu.Count == 0)
                return NotFound("منوی این رستوران یافت نشد.");

            return Ok(menu);
        }
        #endregion
    }
}