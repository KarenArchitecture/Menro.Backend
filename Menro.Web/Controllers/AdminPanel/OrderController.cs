using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Orders.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly ICurrentUserService _currentUserService;

        public OrderController(IAdminOrderService adminOrderService,
            ICurrentUserService currentUserService)
        {
            _adminOrderService = adminOrderService;
            _currentUserService = currentUserService;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var list = await _adminOrderService.GetActiveOrdersAsync(restaurantId);
            return Ok(list);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var list = await _adminOrderService.GetOrderHistoryAsync(restaurantId);
            return Ok(list);
        }


    }
}
