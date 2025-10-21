using Menro.Application.Common.SD;
using Menro.Application.Features.Identity.Services;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly ICurrentUserService _currentUserService;
        public FoodController(IFoodService foodService, ICustomFoodCategoryService cCatService, ICurrentUserService currentUserService)
        {
            _foodService = foodService;
            _cCatService = cCatService;
            _currentUserService = currentUserService;
        }

        // ✅
        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> AddAsync([FromBody] CreateFoodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // گرفتن رستوران کاربر از سرویس کاربر جاری
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var createdFood = await _foodService.AddFoodAsync(dto, restaurantId);

            return Ok(createdFood);
        }

        // ✅
        [HttpGet("read-all")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> GetAllAsync()
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId is not null)
            {
                var foods = await _foodService.GetFoodsListAsync(restaurantId.Value);
                return Ok(foods);
            }
            return BadRequest("User is not a restaurant owner.");
        }

        // ✅
        [HttpGet("{foodId:int}")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> GetAsync(int foodId)
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var food = await _foodService.GetFoodAsync(foodId, restaurantId.Value);
            if (food == null)
                return NotFound();
            return Ok(food);
        }

        
        public async Task<IActionResult> UpdateAsync()
        {

            return Ok();
        }

        // ✅
        [HttpDelete("{foodId:int}")]
        public async Task<IActionResult> DeleteAsync(int foodId)
        {
            var success = await _foodService.DeleteFoodAsync(foodId);
            if (!success)
            {
                return NotFound(new { message = "محصول یافت نشد" });
            }

            return Ok(new { message = "محصول با موفقیت حذف شد" });
        }
        
        
        // ✅
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var categories = await _cCatService.GetCustomFoodCategoriesAsync(restaurantId);
            return Ok(categories);
        }


    }
}
