using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.User
{
    /// <summary>
    /// Provides authenticated user endpoints related to orders,
    /// such as viewing recently ordered foods.
    /// </summary>
    [ApiController]
    [Route("api/user/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;

        public OrderController(IUserRecentOrderCardService recentService)
        {
            _recentService = recentService;
        }

        /// <summary>
        /// Gets a list of recent foods ordered by the authenticated user.
        /// </summary>
        /// <param name="count">Maximum number of recent items (default 8, max 32).</param>
        /// <returns>Recent foods ordered by the user.</returns>
        [HttpGet("recent-foods")]
        [ProducesResponseType(typeof(List<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentFoods([FromQuery] int count = 8)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            count = Math.Clamp(count, 1, 32);
            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }
    }
}
