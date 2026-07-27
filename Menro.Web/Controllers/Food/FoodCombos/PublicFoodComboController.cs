// Web/Controllers/FoodCombos/PublicFoodComboController.cs
using Menro.Application.Features.FoodCombos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.FoodCombos
{
    [ApiController]
    [Route("api/public/restaurant")]
    [AllowAnonymous]
    public class PublicFoodComboController : ControllerBase
    {
        private readonly IFoodComboService _comboService;

        public PublicFoodComboController(IFoodComboService comboService)
        {
            _comboService = comboService;
        }

        // GET: api/public/restaurant/{foodId}/combos
        [HttpGet("{foodId}/combos")]
        public async Task<IActionResult> GetCombos(int foodId)
        {
            var combos = await _comboService.GetPublicCombosAsync(foodId);
            return Ok(combos);
        }
    }
}