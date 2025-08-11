using Menro.Application.FoodCategories.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/restaurant/{restaurantSlug}/foodcategories")]
    public class FoodCategoryController : Controller
    {
        private readonly IShopFoodCategoriesService _categoryService;

        public FoodCategoryController(IShopFoodCategoriesService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesForShop(string restaurantSlug)
        {
            var categories = await _categoryService.GetCategoriesForRestaurantAsync(restaurantSlug);
            return Ok(categories);
        }
    }
}
