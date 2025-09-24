// Menro.Web/Controllers/User/RestaurantController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.User
{
    [ApiController]
    [Route("api/user/restaurant")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)] // hide from Swagger
    public class RestaurantController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;

        public RestaurantController(IUserRecentOrderCardService recentService)
        {
            _recentService = recentService;
        }

        /// <summary>DEPRECATED: use GET /api/user/orders/recent-foods</summary>
        [HttpGet("recent-orders")]
        [Obsolete("Use GET /api/user/orders/recent-foods")]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentOrders([FromQuery] int count = 8)
        {
            // Optional deprecation headers for observability
            Response.Headers["Deprecation"] = "true";
            Response.Headers["Link"] = "</api/user/orders/recent-foods>; rel=\"successor-version\"";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            if (count <= 0) count = 8;
            if (count > 32) count = 32;

            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }
    }
}
