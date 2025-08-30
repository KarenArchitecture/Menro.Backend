using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Menro.Web.Controllers.UserPanel
{
    [ApiController]
    [Route("api/user/[controller]")] // => /api/userpanel/restaurant/...
    [Authorize] // JWT required
    public class RestaurantController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentOrderCardService;

        public RestaurantController(IUserRecentOrderCardService recentOrderCardService)
        {
            _recentOrderCardService = recentOrderCardService;
        }

        // GET /api/userpanel/restaurant/recent-orders?count=8
        [HttpGet("recent-orders")]
        [ProducesResponseType(typeof(List<RestaurantCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RestaurantCardDto>>> GetRecentOrders([FromQuery] int count = 8)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var items = await _recentOrderCardService.GetRecentOrderedRestaurantCardsAsync(userId, count);
            return Ok(items ?? new List<RestaurantCardDto>());
        }
    }
}
