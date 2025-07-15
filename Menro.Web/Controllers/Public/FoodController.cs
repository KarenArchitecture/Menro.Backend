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

        // GET: api/public/food/popular-by-category-random
        [HttpGet("popular-by-category-random")]
        public async Task<ActionResult<PopularFoodCategoryDto>> GetPopularFoodsFromRandomCategory()
        {
            var result = await _foodCardService.GetPopularFoodsFromRandomCategoryAsync();

            if (result == null)
                return NotFound("No food categories found.");

            return Ok(result);
        }
    }
}
