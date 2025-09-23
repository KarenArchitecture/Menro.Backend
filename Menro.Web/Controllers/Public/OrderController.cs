// Menro.Web/Controllers/User/OrderController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.User
{
    [ApiController]
    [Route("api/user/orders")]
    [Authorize] // JWT required
    public class OrderController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;

        public OrderController(IUserRecentOrderCardService recentService)
        {
            _recentService = recentService;
        }

        /// <summary>
        /// Recent foods the current user has ordered (most-recent first, unique by food).
        /// </summary>
        /// <param name="count">Max items (1..32). Defaults to 8.</param>
        [HttpGet("recent-foods")]
        [ProducesResponseType(typeof(List<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentFoods([FromQuery] int count = 8)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            // safety clamp
            if (count <= 0) count = 8;
            if (count > 32) count = 32;

            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }
    }
}
