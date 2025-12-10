using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Implementations;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    /// <summary>
    /// Public-facing food endpoints — used for the homepage sections
    /// such as "Popular Foods" and category-based food listings.
    /// </summary>
    [ApiController]
    [Route("api/public/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IPopularFoodsService _popularFoodsService;
        private readonly IPublicFoodDetailsService _publicFoodDetailsService;

        public FoodController(IPopularFoodsService popularFoodsService, IPublicFoodDetailsService publicFoodDetailsService)
        {
            _popularFoodsService = popularFoodsService;
            _publicFoodDetailsService = publicFoodDetailsService;
        }

        /* ============================================================
           🏠 Home Page - Popular Foods (Lazy-Loaded Rows)
        ============================================================ */

        /// <summary>
        /// Returns a single random Global Food Category (and its top foods)
        /// for the homepage "Popular Foods" lazy-loading section.
        /// </summary>
        /// <param name="foodsPerGroup">Number of foods per category (default 8).</param>
        /// <response code="200">Returns a PopularFoodsDto object containing category and foods.</response>
        [HttpGet("popular")]
        [ProducesResponseType(typeof(PopularFoodsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularFoods([FromQuery] int foodsPerGroup = 8)
        {
            var groups = await _popularFoodsService.GetPopularFoodsGroupsAsync(1, foodsPerGroup);
            var singleGroup = groups.FirstOrDefault();
            if (singleGroup == null)
                return NoContent();

            return Ok(singleGroup);
        }

        /// <summary>
        /// Returns another random Global Food Category (and its top foods)
        /// excluding those already shown on the homepage.
        /// </summary>
        /// <param name="excludeTitles">List of category titles to exclude.</param>
        /// <response code="200">Returns a PopularFoodsDto object containing category and foods.</response>
        [HttpPost("popular-foods-excluding")]
        [ProducesResponseType(typeof(PopularFoodsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularFoodsExcluding([FromBody] List<string> excludeTitles)
        {
            var groups = await _popularFoodsService.GetPopularFoodsGroupsAsync(2, 8); // fetch 2 and pick one new
            var filtered = groups
                .Where(g => !excludeTitles.Contains(g.CategoryTitle))
                .FirstOrDefault();

            if (filtered == null)
                return NoContent();

            return Ok(filtered);
        }

        /* ============================================================
           🏠 Restaurant Page
        ============================================================ */
        /// <summary>
        /// Returns full food details including variants and addons
        /// for the public restaurant page modal.
        /// </summary>
        [HttpGet("{foodId:int}/details")]
        [ProducesResponseType(typeof(PublicFoodDetailDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFoodDetails(int foodId)
        {
            var dto = await _publicFoodDetailsService.GetFoodDetailsAsync(foodId);

            if (dto == null)
                return NotFound("Food not found.");

            return Ok(dto);
        }


        /* ============================================================
           📂 Utility Endpoints (used by analytics or filters)
        ============================================================ */

        /// <summary>
        /// Returns all available Global Category IDs.
        /// Useful for analytics or client-side preselection.
        /// </summary>
        [HttpGet("categories/ids")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoryIds()
        {
            var ids = await _popularFoodsService.GetAllCategoryIdsAsync();
            return Ok(ids ?? new List<int>());
        }

        /// <summary>
        /// Returns popular foods for a specific Global Food Category.
        /// Used when the frontend requests "See more" from one category.
        /// </summary>
        /// <param name="categoryId">Global category ID.</param>
        /// <param name="count">Number of foods to return (default 8).</param>
        /// <response code="200">List of HomeFoodCardDto objects.</response>
        [HttpGet("popular/{categoryId:int}")]
        [ProducesResponseType(typeof(List<HomeFoodCardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularFoodsByCategory([FromRoute] int categoryId, [FromQuery] int count = 8)
        {
            var data = await _popularFoodsService.GetPopularFoodsByCategoryAsync(categoryId, count);
            return Ok(data ?? new List<HomeFoodCardDto>());
        }
    }
}
