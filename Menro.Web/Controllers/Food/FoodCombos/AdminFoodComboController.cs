// Web/Controllers/FoodCombos/AdminFoodComboController.cs
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.FoodCombos.DTOs;
using Menro.Application.Features.FoodCombos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.FoodCombos
{
    [ApiController]
    [Route("api/adminpanel/foodcombo")]
    [Authorize(Roles = SD.Role_Owner)]
    public class AdminFoodComboController : ControllerBase
    {
        private readonly IFoodComboService _comboService;
        private readonly ICurrentUserService _currentUserService;

        public AdminFoodComboController(IFoodComboService comboService, ICurrentUserService currentUserService)
        {
            _comboService = comboService;
            _currentUserService = currentUserService;
        }

        // GET: api/adminpanel/foodcombo/{foodId}
        [HttpGet("{foodId}")]
        public async Task<IActionResult> GetCombos(int foodId)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var comboIds = await _comboService.GetComboFoodIdsAsync(foodId, restaurantId);
            return Ok(comboIds);
        }

        // GET: api/adminpanel/foodcombo/counts
        // Returns { foodId: comboCount } for every food owned by this restaurant,
        // used by the admin rail list to badge foods that already have combos.
        [HttpGet("counts")]
        public async Task<IActionResult> GetComboCounts()
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var counts = await _comboService.GetComboCountsAsync(restaurantId);
            return Ok(counts);
        }

        // PUT: api/adminpanel/foodcombo/{foodId}
        [HttpPut("{foodId}")]
        public async Task<IActionResult> SetCombos(int foodId, [FromBody] SetFoodCombosDto dto)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var (success, error) = await _comboService.SetCombosAsync(foodId, dto.ComboFoodIds ?? new(), restaurantId);

            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "ترکیب‌ها با موفقیت ذخیره شد." });
        }
    }
}