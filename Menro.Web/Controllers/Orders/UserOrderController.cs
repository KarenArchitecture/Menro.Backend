using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.ShowAll.Services.Interfaces;

namespace Menro.Web.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("api/user/orders")]
    public class UserOrderController : ControllerBase
    {
        #region DI
        private readonly IUserRecentOrderCardService _recentService;
        private readonly IRecentOrderBrowseService _recentBrowseService;
        private readonly ICurrentUserService _currentUserService;

        public UserOrderController(
            IUserRecentOrderCardService recentService,
            IRecentOrderBrowseService recentBrowseService,
            ICurrentUserService currentUserService)
        {
            _recentService = recentService;
            _recentBrowseService = recentBrowseService;
            _currentUserService = currentUserService;
        }

        #endregion

        // Homepage: fixed count
        [HttpGet("recent-foods")]
        [ProducesResponseType(typeof(List<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentOrders(
            [FromQuery] int count = 8,
            CancellationToken ct = default)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            count = Math.Clamp(count, 1, 32);

            // NOTE: your current IUserRecentOrderCardService signature doesn't accept ct.
            // That's OK; ct is still valuable for the browse endpoint.
            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);

            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }

        // View All (lazy-load): cursor-based
        [HttpGet("recent-foods/browse")]
        [ProducesResponseType(typeof(PagedResultDto<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResultDto<RecentOrdersFoodCardDto>>> BrowseRecentOrders(
            [FromQuery] int take = 6,
            [FromQuery] string? cursor = null,
            CancellationToken ct = default)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            take = Math.Clamp(take, 1, 24);

            var result = await _recentBrowseService.BrowseRecentOrderedFoodsAsync(userId, take, cursor, ct);

            return Ok(result ?? new PagedResultDto<RecentOrdersFoodCardDto>
            {
                Items = new List<RecentOrdersFoodCardDto>(),
                NextCursor = null,
                HasMore = false
            });
        }
    }
}