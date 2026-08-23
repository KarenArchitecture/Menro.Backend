using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Authorize]
    [Route("api/user/restaurant-rating")]
    public class RestaurantRatingController : ApiControllerBase
    {
        private readonly IRestaurantRatingService _service;
        private readonly ICurrentUserService _currentUserService;

        public RestaurantRatingController(IRestaurantRatingService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] RateRestaurantRequestDto dto, CancellationToken ct)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            try
            {
                var result = await _service.SubmitRatingAsync(userId, dto, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{restaurantId:int}")]
        public async Task<IActionResult> GetMine(int restaurantId, CancellationToken ct)
        {
            var userId = _currentUserService.GetUserId();
            var result = await _service.GetRatingSummaryAsync(userId, restaurantId, ct);
            return Ok(result);
        }
    }
}