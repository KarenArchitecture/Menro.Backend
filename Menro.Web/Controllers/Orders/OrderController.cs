using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Orders
{
    [ApiController]
    [Route("api/public/orders")]
    public class OrdersController : ApiControllerBase
    {
        #region DI
        private readonly IOrderCreationService _orderCreationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOrderHistoryService _orderHistoryService;

        public OrdersController(
            IOrderCreationService orderCreationService,
            ICurrentUserService currentUserService,
            IOrderHistoryService orderHistoryService)
        {
            _orderCreationService = orderCreationService;
            _currentUserService = currentUserService;
            _orderHistoryService = orderHistoryService;
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

        [HttpPost("checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _orderCreationService.CheckoutFromCartAsync(dto, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/bill")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBill(int id, CancellationToken ct)
        {
            var bill = await _orderHistoryService.GetOrderBillAsync(id);
            return bill == null ? NotFound() : Ok(bill);
        }
    }
}