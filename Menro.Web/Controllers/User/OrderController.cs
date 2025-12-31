using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.User
{
    [ApiController]
    [Route("api/user/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;
        private readonly ICurrentUserService _currentUserService;

        public OrderController(IUserRecentOrderCardService recentService, ICurrentUserService currentUserService)
        {
            _recentService = recentService;
            _currentUserService = currentUserService;
        }


        [HttpGet("recent-foods")]
        [ProducesResponseType(typeof(List<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentOrders([FromQuery] int count = 8)
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            count = Math.Clamp(count, 1, 32);
            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }
    }
}
