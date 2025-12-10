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
        #region DI
        private readonly IFoodService _foodService;
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;
        public FoodController(IFoodService foodService,
            ICustomFoodCategoryService cCatService,
            ICurrentUserService currentUserService,
            IFileService fileService,
            IFileUrlService fileUrlService)
        {
            _foodService = foodService;
            _cCatService = cCatService;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _fileUrlService = fileUrlService;
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
            if(!result)
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
            if(food.ImageUrl is not null) food.ImageUrl = _fileUrlService.BuildFoodImageUrl(food.ImageUrl);
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
            if(!ok) return BadRequest(new { message = "خطای ناشناخته‌ای رخ داده" });

            return Ok(new { success = true });
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
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> UploadFoodImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("هیچ فایلی ارسال نشده است.");

            // Allowed extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("فرمت فایل مجاز نیست. فقط jpg, jpeg, png, webp");

            // Max size (مثلاً 2MB)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("حجم فایل نباید بیش از 2 مگابایت باشد.");

            try
            {
                var fileName = await _fileService.UploadFoodImageAsync(file);
                return Ok(fileName);
            }
            catch (Exception ex)
            {
                // می‌تونی Log هم بزنی
                return StatusCode(500, new { message = "خطا در ذخیره‌سازی فایل", error = ex.Message });
            }
        }
    }
}
