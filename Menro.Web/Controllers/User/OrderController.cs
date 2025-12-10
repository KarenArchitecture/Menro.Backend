using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;
using Menro.Application.Common.Interfaces;

namespace Menro.Web.Controllers.User
{
    /// <summary>
    /// Authenticated user endpoints related to orders:
    ///  • Viewing recent ordered foods
    /// </summary>
    [ApiController]
    [Route("api/user/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOrderCreationService _creationService;

        public OrderController(IUserRecentOrderCardService recentService, ICurrentUserService currentUserService, IOrderCreationService creationService)
        {
            _recentService = recentService;
            _currentUserService = currentUserService;
            _creationService = creationService;
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


        /// <summary>
        /// Creates a new order (guest or authenticated).
        /// If the user is authenticated, their userId is attached,
        /// otherwise the order is stored as a guest order.
        /// </summary>
        /// <returns>The created Order Id.</returns>
        [HttpPost("create")]
        [AllowAnonymous] // important: guests can order
        //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = "Payload is required." });

            // Will be null for guests (no JWT), non-null for logged-in users
            string? userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                userId = _currentUserService.GetUserId();
            }

            try
            {
                // IMPORTANT: IOrderCreationService.CreateOrderAsync must accept string? userId
                var orderId = await _creationService.CreateOrderAsync(userId, dto);
                return Ok(orderId);
            }
            catch (Exception ex)
            {
                // TODO: log the exception with your logging system
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
