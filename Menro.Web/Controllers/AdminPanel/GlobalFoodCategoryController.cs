using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.Features.Identity.Services;
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
        public GlobalFoodCategoryController(ICustomFoodCategoryService cCatService, IGlobalFoodCategoryService gCatService, ICurrentUserService currentUserService)
        {
            _cCatService = cCatService;
            _gCatService = gCatService;
            _currentUserService = currentUserService;
        }


        // ✅ - minus svg system!
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
