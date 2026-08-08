using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.SD;
using Menro.Application.Features.Orders.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Orders
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/orders")]
    public class AdminOrderController : ApiControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly ICurrentUserService _currentUserService;

        public AdminOrderController(IAdminOrderService adminOrderService,
            ICurrentUserService currentUserService)
        {
            _adminOrderService = adminOrderService;
            _currentUserService = currentUserService;
        }

        // pendings
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var list = await _adminOrderService.GetActiveOrdersAsync(restaurantId);
            return Ok(list);
        }

        // history
        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var list = await _adminOrderService.GetOrderHistoryAsync(restaurantId);
            return Ok(list);
        }

        // order details
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var dto = await _adminOrderService.GetOrderDetailsAsync(restaurantId, id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /* manage order status */

        // update order status (advances to next step)
        [HttpPut("{id:int}/advance")]
        public async Task<IActionResult> Advance(int id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            try
            {
                var newStatus = await _adminOrderService.AdvanceStatusAsync(restaurantId, id);
                if (newStatus == null) return NotFound();

                return Ok(new { status = newStatus });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // cancel order
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            try
            {
                var newStatus = await _adminOrderService.CancelOrderAsync(restaurantId, id);
                if (newStatus == null) return NotFound();

                return Ok(new { status = newStatus });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var results = await _adminOrderService.SearchOrdersAsync(restaurantId, query ?? "");
            return Ok(results);
        }

    }
}
