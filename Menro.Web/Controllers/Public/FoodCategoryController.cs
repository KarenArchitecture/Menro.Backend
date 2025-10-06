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
    }
}
