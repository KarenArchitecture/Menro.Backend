using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.ShowAll.Services.Interfaces;
using Menro.Application.Features.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IPopularFoodsService _popularFoodsService; // homepage groups
        private readonly IPopularFoodsBrowseService _popularFoodsBrowseService; // ✅ view-all browse
        private readonly IPublicFoodDetailsService _publicFoodDetailsService;

        public FoodController(
            IPopularFoodsService popularFoodsService,
            IPopularFoodsBrowseService popularFoodsBrowseService,
            IPublicFoodDetailsService publicFoodDetailsService)
        {
            _popularFoodsService = popularFoodsService;
            _popularFoodsBrowseService = popularFoodsBrowseService;
            _publicFoodDetailsService = publicFoodDetailsService;
        }

        /* ============================================================
           🏠 Home Page - Popular Foods (Lazy-Loaded Rows)
        ============================================================ */

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

        [HttpPost("popular-foods-excluding")]
        [ProducesResponseType(typeof(PopularFoodsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularFoodsExcluding(
            [FromBody] List<string>? excludeTitles,
            [FromQuery] int foodsPerGroup = 8)
        {
            excludeTitles ??= new List<string>();

            var excludeSet = new HashSet<string>(
                excludeTitles.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            var groups = await _popularFoodsService.GetPopularFoodsGroupsAsync(groupsCount: 50, foodsPerGroup: foodsPerGroup);
            var next = groups.FirstOrDefault(g => !excludeSet.Contains(g.CategoryTitle));

            if (next == null)
                return NoContent();

            return Ok(next);
        }

        /* ============================================================
           ✅ View All (Category-specific) — cursor-based
           GET /api/public/Food/popular/{categoryId}/browse?take=6&cursor=...
        ============================================================ */

        [HttpGet("popular/{categoryId:int}/browse")]
        [ProducesResponseType(typeof(PagedResultDto<HomeFoodCardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BrowsePopularFoodsByCategory(
            [FromRoute] int categoryId,
            [FromQuery] int take = 6,
            [FromQuery] string? cursor = null,
            CancellationToken ct = default)
        {
            var result = await _popularFoodsBrowseService.BrowsePopularFoodsByCategoryAsync(categoryId, take, cursor, ct);
            return Ok(result);
        }

        /* ============================================================
           🏠 Restaurant Page
        ============================================================ */

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
           📂 Utility Endpoints
        ============================================================ */

        [HttpGet("categories/ids")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoryIds()
        {
            var ids = await _popularFoodsService.GetAllCategoryIdsAsync();
            return Ok(ids ?? new List<int>());
        }

        [HttpGet("popular/{categoryId:int}")]
        [ProducesResponseType(typeof(List<HomeFoodCardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPopularFoodsByCategory(
            [FromRoute] int categoryId,
            [FromQuery] int count = 8)
        {
            var data = await _popularFoodsService.GetPopularFoodsByCategoryAsync(categoryId, count);
            return Ok(data ?? new List<HomeFoodCardDto>());
        }
    }
}