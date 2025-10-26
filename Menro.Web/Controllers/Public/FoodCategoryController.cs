using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class FoodCategoryController : ControllerBase
    {
        private readonly IRestaurantPageFoodCategoryService _restaurantPageFoodCategoryService;

        public FoodCategoryController(IRestaurantPageFoodCategoryService restaurantPageFoodCategoryService)
        {
            _restaurantPageFoodCategoryService = restaurantPageFoodCategoryService;
        }

        /// <summary>
        /// Get all global + custom food categories for a restaurant.
        /// </summary>
        /*[HttpGet("{restaurantSlug}")]
        public async Task<ActionResult<List<RestaurantFoodCategoryDto>>> GetRestaurantCategories(string restaurantSlug)
        {
            if (string.IsNullOrWhiteSpace(restaurantSlug))
                return BadRequest();

            var categories = await _restaurantPageFoodCategoryService.GetCategoriesByRestaurantSlugAsync(restaurantSlug);

            // Always return 200 OK with an array (even if empty) so the frontend handles states itself
            return Ok(categories);
        }*/
    }
}
