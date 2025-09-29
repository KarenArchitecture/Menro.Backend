using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Implementations;
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

        /// <summary>
        /// Get global food categories
        /// </summary>
        //[HttpGet("global")]
        //public async Task<ActionResult<List<ShopFoodCategoryDto>>> GetGlobalCategories()
        //{
        //    var categories = await _categoryService.GetGlobalCategoriesAsync();
        //    return Ok(categories);
        //}

        //[HttpGet("")]
        //public async Task<ActionResult<ShopRestaurantFoodCategoriesDto>> GetAllByRestaurantSlug(string restaurantSlug)
        //{
        //    var result = await _categoryService.GetAllForRestaurantAsync(restaurantSlug);
        //    return Ok(result); // should return { restaurant, categories: [{id,name,foods}] }
        //}

        /// <summary>
        /// Get food categories for a specific restaurant
        /// </summary>
        //[HttpGet("restaurant/{restaurantId}")]
        //public async Task<ActionResult<List<ShopFoodCategoryDto>>> GetByRestaurant(int restaurantId)
        //{
        //    var categories = await _categoryService.GetByRestaurantAsync(restaurantId);
        //    return Ok(categories);
        //}

        /// <summary>
        /// Get all food categories (global + restaurant-specific) for a restaurant page
        /// </summary>
        //[HttpGet("")]
        //public async Task<ActionResult<List<ShopFoodCategoryDto>>> GetAllForRestaurant(string restaurantSlug)
        //{
        //    var categories = await _categoryService.GetAllForRestaurantAsync(restaurantSlug);
        //    return Ok(categories);
        //}
    }
}
