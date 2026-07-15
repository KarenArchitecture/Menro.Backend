using Menro.Application.Common.SD;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/admin/restaurants")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminRestaurantController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public AdminRestaurantController(IRestaurantService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRestaurantsListForAdminAsync([FromQuery] RestaurantStatus status)
        {
            var result = await _service.GetRestaurantsListForAdminAsync(status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantDetailsForAdmin(int id)
        {
            var result = await _service.GetRestaurantDetailsForAdminAsync(id);
            if (result == null) return NotFound("Restaurant not found");

            return Ok(result);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveRestaurant(ApproveRestaurantDto dto)
        {
            var ok = await _service.ApproveRestaurantAsync(dto.RestaurantId, dto.Approve);
            if (!ok) return NotFound("Restaurant not found");

            return Ok(new { message = "Updated successfully" });
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus(UpdateRestaurantStatusDto dto)
        {
            var ok = await _service.UpdateRestaurantStatusAsync(dto.RestaurantId, dto.Status, dto.RejectReason);

            if (!ok)
                return NotFound("Restaurant not found");

            return Ok(new { message = "Status updated successfully" });
        }



    }
}
