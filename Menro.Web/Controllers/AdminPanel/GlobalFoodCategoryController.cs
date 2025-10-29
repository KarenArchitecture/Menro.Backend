using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.FoodCategories.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class GlobalFoodCategoryController : ControllerBase
    {
        private readonly ICustomFoodCategoryService _cCatService;
        private readonly IGlobalFoodCategoryService _gCatService;
        private readonly ICurrentUserService _currentUserService;
        public GlobalFoodCategoryController(
            ICustomFoodCategoryService cCatService, 
            IGlobalFoodCategoryService gCatService, 
            ICurrentUserService currentUserService
            )
        {
            _cCatService = cCatService;
            _gCatService = gCatService;
            _currentUserService = currentUserService;
        }

        //✅
        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> AddAsync([FromBody] CreateGlobalCategoryDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "نام دسته‌بندی الزامی است." });

            var result = await _gCatService.AddGlobalCategoryAsync(dto);
            if (!result)
                return BadRequest(new { message = "افزودن دسته‌بندی موفق نبود (ممکن است تکراری باشد)." });

            return Ok(new { message = "دسته‌بندی با موفقیت اضافه شد." });
        }

        // ✅
        // read-all
        [HttpGet("read-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _gCatService.GetAllGlobalCategoriesAsync();
            return Ok(list);
        }

        // ✅
        // read
        [HttpGet("read")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetAsync([FromQuery] int catId)
        {
            try
            {
                var cat = await _gCatService.GetGlobalCategoryAsync(catId);
                return Ok(cat);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ✅
        // update
        [HttpPut("update")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateGlobalCategoryDto dto)
        {
            try
            {
                await _gCatService.UpdateGlobalCategoryAsync(dto);
                return Ok(new { message = "Global category updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅
        [HttpDelete("delete/{catId}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> DeleteAsync(int catId)
        {
            var result = await _gCatService.DeleteGlobalCategoryAsync(catId);
            if (!result)
                return BadRequest(new { message = "حذف دسته‌بندی موفق نبود." });

            return Ok(new { message = "دسته‌بندی حذف شد." });
        }


    }
}
