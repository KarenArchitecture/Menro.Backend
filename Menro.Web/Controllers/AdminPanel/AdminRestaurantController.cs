using Menro.Application.Common.SD;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/admin/restaurants")]
    public class AdminRestaurantController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public AdminRestaurantController(IRestaurantService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetRestaurantsListForAdminAsync([FromQuery] bool? approved)
        {
            var result = await _service.GetRestaurantsListForAdminAsync(approved);
            return Ok(result);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = SD.Role_Admin)]
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


    }
}
