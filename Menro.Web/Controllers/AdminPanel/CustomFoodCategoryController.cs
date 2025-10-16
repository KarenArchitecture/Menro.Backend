using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.Features.Identity.Services;
using Menro.Application.FoodCategories.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class CustomFoodCategoryController : ControllerBase
    {
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly IGlobalFoodCategoryService _gCatService;
        private readonly ICurrentUserService _currentUserService;
        public CustomFoodCategoryController(ICustomFoodCategoryService cCatService, IGlobalFoodCategoryService gCatService, ICurrentUserService currentUserService)
        {
            _cCatService = cCatService;
            _gCatService = gCatService;
            _currentUserService = currentUserService;
        }

        [HttpPost("add-from-global")]
        [Authorize]
        public async Task<IActionResult> AddFromGlobalsAsync(int globalCategoryId, int restaurantId)
        {
            bool res = await _cCatService.AddFromGlobalAsync(globalCategoryId, restaurantId);

            if (!res)
            {
                return BadRequest(new { message = "خطا در افزودن دسته‌بندی از دسته‌بندی‌های عمومی." });
            }

            return Ok(new { message = "دسته‌بندی با موفقیت اضافه شد." });

        }

        [HttpGet("read-all")]
        [Authorize]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId is not null)
            {
                var catList = await _cCatService.GetCustomFoodCategoriesAsync(restaurantId.Value);
                return Ok(catList);
            }
            return BadRequest(new { message = "بارگیری دسته بندی ها ناموفق بود" });
        }
    }
}
