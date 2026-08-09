// Web/Controllers/Restaurants/AdminRestaurantPaymentController.cs
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
    [Route("api/admin/restaurant/payment")]
    public class AdminRestaurantPaymentController : ApiControllerBase
    {
        private readonly IRestaurantPaymentSettingsService _service;
        private readonly ICurrentUserService _currentUserService;

        public AdminRestaurantPaymentController(
            IRestaurantPaymentSettingsService service,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            return Ok(await _service.GetAsync(restaurantId));
        }

        [HttpPut]
        public async Task<IActionResult> Set([FromBody] UpdateRestaurantPaymentMethodDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            try
            {
                await _service.SetAsync(restaurantId, dto);
                return Ok(new { message = "شیوه پرداخت به‌روزرسانی شد." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}