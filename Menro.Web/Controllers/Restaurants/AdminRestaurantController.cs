using Menro.Application.Common.SD;
using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)]
    [Route("api/admin/restaurants")]
    public class AdminRestaurantController : ApiControllerBase
    {
        private readonly IAdminRestaurantService _service;

        public AdminRestaurantController(IAdminRestaurantService service)
        {
            _service = service;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetRestaurantsOverview(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetRestaurantsOverviewAsync(search, categoryId, page, pageSize);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRestaurantsListForAdminAsync([FromQuery] RestaurantStatus status)
        {
            var result = await _service.GetRestaurantsListForAdminAsync(status);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRestaurantDetailsForAdmin(int id)
        {
            var result = await _service.GetRestaurantDetailsForAdminAsync(id);
            if (result == null) return NotFound("Restaurant not found");

            return Ok(result);
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus(UpdateRestaurantStatusDto dto)
        {
            var ok = await _service.UpdateRestaurantStatusAsync(dto.RestaurantId, dto.Status, dto.RejectReason);
            if (!ok)
                return BadRequest("درخواست معتبر نیست یا قبلاً بررسی شده است.");
            return Ok(new { message = "Status updated successfully" });
        }



    }
}
