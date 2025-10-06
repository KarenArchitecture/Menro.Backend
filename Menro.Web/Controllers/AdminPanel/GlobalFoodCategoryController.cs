using Menro.Application.Common.SD;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class GlobalFoodCategoryController : ControllerBase
    {
        private readonly IGlobalFoodCategoryService _service;

        public GlobalFoodCategoryController(IGlobalFoodCategoryService service)
        {
            _service = service;
        }

        // create
        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<ActionResult<GlobalCategoryDTO>> Create([FromBody] CreateGlobalCategoryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _service.CreateCategoryAsync(dto))
            {
                return Ok();
            }
            return BadRequest("افزودن غذا ناموفق بود");
            
        }

        // read-all
        [HttpGet("read-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllCategoriesAsync();
            return Ok(list);
        }

        // read
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var cat = await _service.GetGlobalCategoryAsync(id);
                return Ok(cat);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
