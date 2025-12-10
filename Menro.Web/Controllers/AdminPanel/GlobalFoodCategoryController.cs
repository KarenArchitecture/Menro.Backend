using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.GlobalFoodCategories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize(Roles = SD.Role_Admin)]
    public class GlobalFoodCategoryController : ControllerBase
    {
        #region DI
        private readonly IGlobalFoodCategoryService _gCatService;
        private readonly IFileUrlService _fileUrlService;
        public GlobalFoodCategoryController(IGlobalFoodCategoryService gCatService,
            IFileUrlService fileUrlService)
        {
            _gCatService = gCatService;
            _fileUrlService = fileUrlService;
        }

        #endregion


        [HttpPost("add")]
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
        [HttpGet("read-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _gCatService.GetAllGlobalCategoriesAsync();
            list.ForEach(cat =>
            {
                cat.Icon!.Url = _fileUrlService.BuildIconUrl(cat.Icon.Url);
            });
            return Ok(list);
        }

        // ✅
        [HttpGet("read")]
        public async Task<IActionResult> GetAsync([FromQuery] int catId)
        {
            try
            {
                var cat = await _gCatService.GetGlobalCategoryAsync(catId);
                cat.Icon!.Url = _fileUrlService.BuildIconUrl(cat.Icon.Url);
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
        public async Task<IActionResult> DeleteAsync(int catId)
        {
            var result = await _gCatService.DeleteGlobalCategoryAsync(catId);
            if (!result)
                return BadRequest(new { message = "حذف دسته‌بندی موفق نبود." });

            return Ok(new { message = "دسته‌بندی حذف شد." });
        }


    }
}
