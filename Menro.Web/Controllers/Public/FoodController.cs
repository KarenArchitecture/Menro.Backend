using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/food")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodCardService _foodCardService;

        public FoodController(IFoodCardService foodCardService)
        {
            _foodCardService = foodCardService;
        }

        /// <summary>
        /// Gets popular foods from a random category (without excluding any).
        /// GET: api/public/food/popular-by-category-random
        /// </summary>
        [HttpGet("popular-by-category-random")]
        public async Task<ActionResult<PopularFoodCategoryDto>> GetPopularFoodsFromRandomCategory()
        {
            var result = await _foodCardService.GetPopularFoodsFromRandomCategoryAsync();

            if (result == null)
                return Ok(null); // ✅ Return 200 with null

            return Ok(result);
        }

        /// <summary>
        /// Gets popular foods from a random category, excluding already-used category titles.
        /// POST: api/public/food/popular-by-category-random
        /// Body: ["Category A", "Category B", ...]
        /// </summary>
        [HttpPost("popular-by-category-random")]
        public async Task<ActionResult<PopularFoodCategoryDto>> GetPopularFoodsFromRandomCategoryExcluding([FromBody] List<string> usedCategories)
        {
            var result = await _foodCardService.GetPopularFoodsFromRandomCategoryExcludingAsync(usedCategories);

            if (result == null)
                return Ok(null);

            return Ok(result);
        }
    }
}
