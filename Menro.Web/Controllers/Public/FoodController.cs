using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IPopularFoodsService _popularFoodsService;

        public FoodController(IPopularFoodsService popularFoodsService)
        {
            _popularFoodsService = popularFoodsService;
        }

        #region Home Page
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
                    IconId = null,
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
        #endregion


        #region Restaurant Page

        #endregion

    }
}
