using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Implementations;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IPopularFoodsService _popularFoodsService;
        private readonly IRestaurantMenuService _restaurantService;

        public FoodController(IPopularFoodsService popularFoodsService, IRestaurantMenuService restaurantService)
        {
            _popularFoodsService = popularFoodsService;
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Gets popular foods from a random category (without excluding any).
        /// GET: api/public/food/popular-foods
        /// </summary>
        //[HttpGet("popular-foods")]
        //public async Task<ActionResult<PopularFoodsDto>> GetPopularFoodsFromRandomCategory()
        //{
        //    var result = await _popularFoodsService.GetPopularFoodsFromRandomCategoryAsync();

        //    if (result == null)
        //        return Ok(null); // ✅ Return 200 with null

        //    return Ok(result);
        //}

        [HttpGet("popular-foods")]
        public async Task<ActionResult<PopularFoodsDto>> GetPopularFoods()
        {
            var result = await _popularFoodsService.GetPopularFoodsFromRandomCategoryAsync();

            // If no popular foods exist, return an empty DTO instead of null
            if (result == null)
            {
                return Ok(new PopularFoodsDto
                {
                    CategoryTitle = string.Empty,
                    SvgIcon = string.Empty,
                    Foods = new List<HomeFoodCardDto>()
                });
            }

            return Ok(result);
        }
        /// <summary>
        /// Gets popular foods from a random category, excluding already-used category titles.
        /// POST: api/public/food/popular-foods
        /// Body: ["Category A", "Category B", ...]
        /// </summary>
        [HttpPost("popular-foods-excluding")]
        public async Task<ActionResult<PopularFoodsDto>> GetPopularFoodsFromRandomCategoryExcluding([FromBody] List<string> usedCategories)
        {
            var result = await _popularFoodsService.GetPopularFoodsFromRandomCategoryExcludingAsync(usedCategories);

            if (result == null)
                return Ok(null);

            return Ok(result);
        }

    }
}
