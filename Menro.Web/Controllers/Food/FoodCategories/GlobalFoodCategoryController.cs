using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.GlobalFoodCategories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Food.FoodCategories
{
    [ApiController]
    [Route("api/adminpanel/globalFoodCategory")]
    [Authorize(Roles = SD.Role_Admin)]
    public class GlobalFoodCategoryController : ApiControllerBase
    {
        #region DI
        private readonly IGlobalFoodCategoryService _gCatService;
        public GlobalFoodCategoryController(IGlobalFoodCategoryService gCatService)
        {
            _gCatService = gCatService;
        }

        #endregion


        [HttpPost("add")]
        public async Task<IActionResult> AddAsync([FromBody] CreateGlobalCategoryDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "نام دسته‌بندی الزامی است." });

            var result = await _gCatService.AddGlobalCategoryAsync(dto);

            return FromResult(result, "دسته‌بندی با موفقیت اضافه شد.");
        }

        // ✅
        [HttpGet("read-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _gCatService.GetAllGlobalCategoriesAsync();
            return Ok(list);
        }

        // ✅
        [HttpGet("read")]
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
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateGlobalCategoryDto dto)
        {
            var result = await _gCatService.UpdateGlobalCategoryAsync(dto);
            return FromResult(result, "دسته‌بندی با موفقیت ویرایش شد.");
        }

        // ✅
        [HttpDelete("delete/{catId}")]
        public async Task<IActionResult> DeleteAsync(int catId)
        {
            var result = await _gCatService.DeleteGlobalCategoryAsync(catId);
            if (!result)
                return BadRequest(new { message = "حذف دسته‌بندی موفق نبود." });

            return Ok(new { message = "دسته‌بندی حذف شد." });
        }


    }
}
