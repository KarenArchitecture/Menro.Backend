using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.SD;
using Menro.Application.Features.FoodCategories.Services.Interfaces;
using Menro.Application.Features.Foods.DTOs;
using Menro.Application.Features.Foods.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Food
{
    [ApiController]
    [Route("api/admin/food")]
    [Authorize(Roles = SD.Role_Owner)]
    public class AdminFoodController : ApiControllerBase
    {
        #region DI
        private readonly IFoodService _foodService;
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly ICurrentUserService _currentUserService;

        public AdminFoodController(IFoodService foodService,
            ICustomFoodCategoryService cCatService,
            ICurrentUserService currentUserService)
        {
            _foodService = foodService;
            _cCatService = cCatService;
            _currentUserService = currentUserService;
        }


        #endregion



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
            bool result = await _foodService.AddFoodAsync(dto, restaurantId);
            if (!result)
                return BadRequest(new { message = "خطای ناشناخته‌ای رخ داده است" });
            return Ok();
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
            var food = await _foodService.GetFoodDetailsAsync(foodId, restaurantId.Value);
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

            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var ok = await _foodService.UpdateFoodAsync(dto);
            if (!ok) return BadRequest(new { message = "خطای ناشناخته‌ای رخ داده" });

            return Ok(new { success = true });
        }

        [HttpPatch("toggle-status/{foodId:int}")]
        public async Task<IActionResult> ToggleStatus(int foodId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _foodService.ToggleFoodStatusAsync(foodId, restaurantId);

            if (!result)
                return NotFound();

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
        public async Task<IActionResult> GetCategoriesAsync()
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var categories = await _cCatService.GetCustomFoodCategoriesAsync(restaurantId);
            return Ok(categories);
        }

        // ✅
        [HttpPost("upload-food-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFoodImage(
            [FromForm] UploadFoodImageDto dto)
        {
            try
            {
                var result = await _foodService.UploadFoodImageAsync(dto.File);
                return Ok(new { fileName = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در ذخیره‌سازی فایل", error = ex.Message });
            }
        }
    }
}
