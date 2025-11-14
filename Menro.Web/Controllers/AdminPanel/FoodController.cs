using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize(Roles = SD.Role_Owner)]
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
        public async Task<IActionResult> AddAsync([FromBody] CreateFoodDto dto)
        {
            // validations
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto.HasVariants)
            {
                if (dto.Variants == null || dto.Variants.Count == 0)
                    return BadRequest(new { message = "حداقل یک نوع غذا باید تعریف شود" });
                
                var defaults = dto.Variants.Count(v => v.IsDefault);
                if (defaults == 0)
                    return BadRequest(new { message = "حداقل یک نوع باید پیش فرض باشد" });
                    
                if (defaults > 1)
                    return BadRequest(new { message = "فقط یک نوع می‌تواند پیش فرض باشد" });
            }

            // گرفتن رستوران کاربر از سرویس کاربر جاری
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var createdFood = await _foodService.AddFoodAsync(dto, restaurantId);

            return Ok(createdFood);
        }

        // ✅
        [HttpGet("read-all")]
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
        public async Task<IActionResult> GetAsync(int foodId)
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var food = await _foodService.GetFoodAsync(foodId, restaurantId.Value);
            if (food == null)
                return NotFound();
            return Ok(food);
        }

        // ✅
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateFoodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var res = await _foodService.UpdateFoodAsync(dto);
            if (!res)
            {
                return BadRequest("ویرایش غذا ناموفق بود");
            }
            return Ok(res);
        }

        // ✅
        [HttpDelete("{foodId:int}")] // change it to "read" later
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
        public async Task<IActionResult> GetCategoriesAsync()
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var categories = await _cCatService.GetCustomFoodCategoriesAsync(restaurantId);
            return Ok(categories);
        }


    }
}
