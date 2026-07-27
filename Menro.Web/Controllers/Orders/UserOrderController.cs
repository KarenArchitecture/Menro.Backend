using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Application.Features.Search.DTOs;

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
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly ICurrentUserService _currentUserService;

        public UserOrderController(
            IUserRecentOrderCardService recentService,
            IRecentOrderBrowseService recentBrowseService,
            IOrderHistoryService orderHistoryService,
            ICurrentUserService currentUserService)
        {
            _recentService = recentService;
            _recentBrowseService = recentBrowseService;
            _orderHistoryService = orderHistoryService;
            _currentUserService = currentUserService;
        }
        #endregion

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
            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
        }

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

        // Order history for /orders page
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<UserOrderListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<UserOrderListItemDto>>> GetHistory()
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var orders = await _orderHistoryService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }
    }
}