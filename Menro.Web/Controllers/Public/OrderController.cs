// Menro.Web.Controllers.Public
using Menro.Application.Common.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/public/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderCreationService _orderCreationService;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(
        IOrderCreationService orderCreationService,
        ICurrentUserService currentUserService)
    {
        _orderCreationService = orderCreationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Create an order (guest or logged-in).
    /// </summary>
    [HttpPost("create")]
    [Authorize] // guests allowed
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
            // you can refine this later (map domain exceptions to nicer payloads)
            return BadRequest(new { message = ex.Message });
        }
    }
}
