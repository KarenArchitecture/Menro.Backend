using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/owner/restaurant")]
    public class OwnerRestaurantController : ApiControllerBase
    {
        #region DI
        private readonly IRestaurantService _service;
        private readonly ICurrentUserService _currentUserService;

        public OwnerRestaurantController(
            IRestaurantService service,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }
        #endregion

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _service.GetRestaurantProfileAsync(restaurantId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateRestaurantProfileDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            dto.Id = restaurantId;

            await _service.UpdateRestaurantProfileAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpGet("check-slug")]
        public async Task<IActionResult> CheckSlugAvailability([FromQuery] string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest(new { message = "اسلاگ نمی‌تواند خالی باشد." });

            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var isAvailable = await _service.IsSlugAvailableAsync(slug, restaurantId);

            return Ok(new { available = isAvailable });
        }

        [HttpGet("context")]
        public async Task<IActionResult> Get()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            return Ok(new { restaurantId });
        }
    }
}