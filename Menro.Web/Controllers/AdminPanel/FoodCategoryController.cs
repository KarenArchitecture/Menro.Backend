using Menro.Application.Common.SD;
using Menro.Application.Features.Identity.Services;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Application.Services.Implementations;
using Menro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class FoodCategoryController : ControllerBase
    {
        private readonly IFoodCategoryService _foodCategoryService;
        private readonly ICurrentUserService _currentUserService;
        public FoodCategoryController(IFoodCategoryService foodCategoryService, ICurrentUserService currentUserService)
        {
            _foodCategoryService = foodCategoryService;
            _currentUserService = currentUserService;
        }

        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> Create()
        {
            return Ok();
        }
        
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            //var categories = await _foodCategoryService.GetAllAsync();
            //return Ok(categories);
            return Ok();
        }

        [HttpGet("read-all")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> ReadAll()
        {
            return BadRequest("User is not a restaurant owner.");
        }

        [HttpGet("{foodCategoryId:int}")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> Read(int foodCategoryId)
        {
            return Ok();
        }

        public IActionResult Update()
        {
            return Ok();
        }

        [HttpDelete("{foodCategoryId:int}")]
        public async Task<IActionResult> Delete(int? foodCategoryId)
        {
            return Ok(new { message = "محصول با موفقیت حذف شد" });
        }




    }
}
