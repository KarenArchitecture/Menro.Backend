using Menro.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;
using Menro.Application.Features.Identity.Services;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.FoodCategories.DTOs;
using Menro.Application.Common.Interfaces;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class RestaurantController : ControllerBase
    {
        #region Dependency Injection

        private readonly IRestaurantService _restaurantService;
        private readonly IRandomRestaurantCardService _randomRestaurantCardService;
        private readonly IRestaurantBannerService _restaurantBannerService;
        private readonly IUserService _userService;
        private readonly IRestaurantBannerService _bannerService;
        private readonly IRestaurantMenuService _menuService;
        private readonly IRestaurantPageFoodCategoryService _restaurantPageFoodCategoryService;
        private readonly IFileUrlService _fileUrlService;

        public RestaurantController(
            IRestaurantService restaurantService,
            IRandomRestaurantCardService randomRestaurantCardService,
            IRestaurantBannerService restaurantBannerService,
            IUserService userService,
            IRestaurantPageFoodCategoryService restaurantPageFoodCategoryService,
            IRestaurantMenuService menuService,
            IFileUrlService fileUrlService)
        {
            _restaurantService = restaurantService;
            _randomRestaurantCardService = randomRestaurantCardService;
            _restaurantBannerService = restaurantBannerService;
            _userService = userService;
            _menuService = menuService;
            _restaurantPageFoodCategoryService = restaurantPageFoodCategoryService;
            _fileUrlService = fileUrlService;
        }

        #endregion


        #region Home Page Endpoints
        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRandomRestaurants()
        {
            var result = await _randomRestaurantCardService.GetRandomRestaurantCardsAsync();
            return Ok(result);
        }
        #endregion


        #region Registration & Global Categories

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

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _restaurantService.GetRestaurantCategoriesAsync();
            return Ok(categories);
        }

        #endregion


        #region Restaurant Page Endpoints
        [HttpGet("{slug}/banner")]
        public async Task<ActionResult<RestaurantBannerDto?>> GetBanner(string slug)
        {
            var banner = await _restaurantBannerService.GetBannerBySlugAsync(slug);
            if (banner == null)
                return NotFound();

            return Ok(banner);
        }

        [HttpGet("{slug}/categories")]
        public async Task<ActionResult<List<RestaurantFoodCategoryDto>>> GetRestaurantCategoriesBySlug(string slug, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug cannot be empty.");
            
            var categories = await _restaurantPageFoodCategoryService.GetRestaurantCategoriesAsync(slug, ct);
            
            // + //
            categories.ForEach(cat =>
            {
                cat.SvgIcon = _fileUrlService.BuildIconUrl(cat.SvgIcon);
            });
            if (categories == null || categories.Count == 0)
                return NotFound("هیچ دسته‌ای برای این رستوران یافت نشد.");

            return Ok(categories);
        }

        
        [HttpGet("{slug}/menu")]
        public async Task<ActionResult<List<RestaurantMenuDto>>> GetRestaurantMenuBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug cannot be empty.");

            var menu = await _menuService.GetMenuBySlugAsync(slug);

            if (menu == null || menu.Count == 0)
                return NotFound("منوی این رستوران یافت نشد.");

            return Ok(menu);
        }


        #endregion
    }
}
