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
        public async Task<IActionResult> AddGlobalCategoryAsync([FromBody] CreateGlobalCategoryDTO dto)
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
        public async Task<IActionResult> GetAllGlobalCategories()
        {
            var list = await _gCatService.GetAllGlobalCategoriesAsync();
            return Ok(list);
        }


        // read
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var cat = await _gCatService.GetGlobalCategoryAsync(id);
                return Ok(cat);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
