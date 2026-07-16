using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Menro.Web.Controllers.Orders
{
    [ApiController]
    [Route("api/public/orders")]
    public class OrdersController : ControllerBase
    {
        #region DI
        private readonly IOrderCreationService _orderCreationService;
        private readonly ICurrentUserService _currentUserService;

        public OrdersController(
            IOrderCreationService orderCreationService,
            ICurrentUserService currentUserService)
        {
            _orderCreationService = orderCreationService;
            _currentUserService = currentUserService;
        }

        #endregion

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.GetUserId();

            try
            {
                var orderId = await _orderCreationService.CreateOrderAsync(userId, dto);
                return Ok(new { orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}

