using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Application.Common.SD;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    // Separate controller from AdminRestaurantController on purpose:
    // AdminRestaurantController manages individual restaurants (approve/reject/list),
    // this one manages the global "restaurant type" taxonomy
    // (e.g. کافه، فست‌فودی، ...) shown in RegisterRestaurantPage's dropdown.
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)]
    [Route("api/admin/restaurant-categories")]
    public class AdminRestaurantCategoryController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public AdminRestaurantCategoryController(IRestaurantService service)
        {
            _service = service;
        }

        [HttpGet("read-all")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetRestaurantCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("read")]
        public async Task<IActionResult> GetById([FromQuery] int catId)
        {
            var category = await _service.GetRestaurantCategoryByIdAsync(catId);
            if (category == null) return NotFound(new { message = "دسته‌بندی یافت نشد" });

            return Ok(category);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantCategoryDto dto)
        {
            var (success, error) = await _service.CreateRestaurantCategoryAsync(dto);
            if (!success) return BadRequest(new { message = error });

            return Ok(new { message = "دسته‌بندی با موفقیت ایجاد شد" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateRestaurantCategoryDto dto)
        {
            var (success, error) = await _service.UpdateRestaurantCategoryAsync(dto);
            if (!success) return BadRequest(new { message = error });

            return Ok(new { message = "دسته‌بندی با موفقیت ویرایش شد" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _service.DeleteRestaurantCategoryAsync(id);
            if (!success) return BadRequest(new { message = error });

            return Ok(new { message = "دسته‌بندی با موفقیت حذف شد" });
        }
    }
}
