//using System.Security.Claims;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Menro.Application.Orders.DTOs;
//using Menro.Application.Orders.Services.Interfaces;

//namespace Menro.Web.Controllers.User
//{
//    /// <summary>
//    /// Provides authenticated user endpoints related to orders,
//    /// including:
//    ///  • Creating an order from checkout
//    ///  • Viewing recent ordered foods
//    /// </summary>
//    [ApiController]
//    [Route("api/user/orders")]
//    [Authorize]
//    public class OrderController : ControllerBase
//    {
//        private readonly IUserRecentOrderCardService _recentService;
//        private readonly IOrderCreationService _creationService;

//        public OrderController(
//            IUserRecentOrderCardService recentService,
//            IOrderCreationService creationService)
//        {
//            _recentService = recentService;
//            _creationService = creationService;
//        }

//        /// <summary>
//        /// Creates a new order for the authenticated user from the checkout payload.
//        /// </summary>
//        /// <returns>The created Order Id.</returns>
//        [HttpPost("create")]
//        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderDto dto)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (string.IsNullOrWhiteSpace(userId))
//                return Unauthorized();

//            if (dto == null)
//                return BadRequest(new { error = "Payload is required." });

//            try
//            {
//                var orderId = await _creationService.CreateOrderAsync(userId, dto);
//                return Ok(orderId);
//            }
//            catch (Exception ex)
//            {
//                // You can log ex here with your logging system
//                return BadRequest(new { error = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Gets a list of recent foods ordered by the authenticated user.
//        /// </summary>
//        /// <param name="count">Maximum number of recent items (default 8, max 32).</param>
//        /// <returns>Recent foods ordered by the user.</returns>
//        [HttpGet("recent-foods")]
//        [ProducesResponseType(typeof(List<RecentOrdersFoodCardDto>), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        public async Task<ActionResult<List<RecentOrdersFoodCardDto>>> GetRecentFoods([FromQuery] int count = 8)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (string.IsNullOrWhiteSpace(userId))
//                return Unauthorized();

//            count = Math.Clamp(count, 1, 32);
//            var items = await _recentService.GetUserRecentOrderedFoodsAsync(userId, count);
//            return Ok(items ?? new List<RecentOrdersFoodCardDto>());
//        }
//    }
//}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;

namespace Menro.Web.Controllers.Public
{
    /// <summary>
    /// Public endpoints for order creation from checkout.
    /// Works for both guests and logged-in users.
    /// </summary>
    [ApiController]
    [Route("api/public/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderCreationService _creationService;

        public OrdersController(IOrderCreationService creationService)
        {
            _creationService = creationService;
        }

        
    }
}

