using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.User
{
    /// <summary>
    /// (Deprecated) Legacy endpoint for user recent restaurant orders.
    /// Replaced by <see cref="OrderController.GetRecentFoods"/>.
    /// </summary>
    [ApiController]
    [Route("api/user/[controller]")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RestaurantController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;

        public RestaurantController(IUserRecentOrderCardService recentService)
        {
            _recentService = recentService;
        }

        /// <summary>
        /// DEPRECATED: Use GET /api/user/orders/recent-foods instead.
        /// </summary>
        [HttpGet("recent-orders")]
        [Obsolete("Use GET /api/user/orders/recent-foods")]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentOrders([FromQuery] int count = 8)
        {
            Response.Headers["Deprecation"] = "true";
            Response.Headers["Link"] = "</api/user/orders/recent-foods>; rel=\"successor-version\"";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            count = Math.Clamp(count, 1, 32);
            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }
    }
}
