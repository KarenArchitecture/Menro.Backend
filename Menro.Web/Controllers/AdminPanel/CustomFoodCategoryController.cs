using Menro.Application.Features.CustomFoodCategory.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.Common.SD;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Common.Interfaces;


namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize(Roles = SD.Role_Owner)]
    public class CustomFoodCategoryController : ControllerBase
    {
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly ICurrentUserService _currentUserService;
        public CustomFoodCategoryController(
            ICustomFoodCategoryService cCatService, 
            ICurrentUserService currentUserService)
        {
            _cCatService = cCatService;
            _currentUserService = currentUserService;
        }


        // ✅
        [HttpPost("add")]
        public async Task<IActionResult> AddAsync (CreateCustomFoodCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "نام دسته‌بندی الزامی است." });
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _cCatService.AddCategoryAsync(dto, restaurantId);
            if (!result)
                return BadRequest(new { message = "افزودن دسته‌بندی موفق نبود (ممکن است تکراری باشد)." });

            return Ok(new { message = "دسته‌بندی با موفقیت اضافه شد." });
        }

        // ✅
        [HttpPost("add-from-global")]
        public async Task<IActionResult> AddFromGlobalsAsync([FromQuery] int globalCategoryId)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (!await _cCatService.AddFromGlobalAsync(globalCategoryId, restaurantId))
            {
                return BadRequest(new { message = "خطا در افزودن دسته‌بندی از دسته‌بندی‌های عمومی." });
            }

            return Ok(new { message = "دسته‌بندی با موفقیت اضافه شد." });

        }

        // ✅
        [HttpGet("read-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId is not null)
            {
                var catList = await _cCatService.GetAllCustomFoodCategoriesAsync(restaurantId.Value);
                return Ok(catList);
            }
            return BadRequest(new { message = "بارگیری دسته بندی ها ناموفق بود" });
        }

        // ✅
        [HttpGet("read")]
        public async Task<IActionResult> GetAsync([FromQuery] int catId)
        {
            var category = await _cCatService.GetCategoryAsync(catId);
            if (category == null)
                return NotFound(new { message = "دسته‌بندی یافت نشد." });

            return Ok(category);
        }

        // ✅
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateCustomFoodCategoryDto dto)
        {
            try
            {
                await _cCatService.UpdateCategoryAsync(dto);
                return Ok(new { message = "Global category updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅
        [HttpDelete("delete/{catId}")]
        public async Task<IActionResult> DeleteAsync(int catId)
        {
            var result = await _cCatService.DeleteCustomCategoryAsync(catId);
            if (!result)
                return BadRequest(new { message = "حذف دسته‌بندی موفق نبود." });

            return Ok(new { message = "دسته‌بندی حذف شد." });
        }


    }
}
