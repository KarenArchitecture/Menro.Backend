using Menro.Application.Features.Cart.DTOs;
using Menro.Application.Features.Cart.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Cart
{
    [ApiController]
    [Route("api/public/cart")]
    public class CartController : ControllerBase
    {
        #region DI
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        #endregion

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var cart = await _cartService.GetCartAsync(ct);
            return Ok(cart);
        }

        [HttpPut("items")]
        [AllowAnonymous]
        public async Task<IActionResult> SetItem([FromBody] SetCartItemRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _cartService.SetItemAsync(dto, ct);

                if (result.RequiresConfirmation)
                    return Conflict(result); // 409 -> frontend shows the restaurant-switch modal

                return Ok(result.Cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            await _cartService.ClearCartAsync(ct);
            return NoContent();
        }

        [HttpPost("merge")]
        [Authorize]
        public async Task<IActionResult> MergeGuestCart(CancellationToken ct)
        {
            var cart = await _cartService.MergeGuestCartAsync(ct);
            return Ok(cart);
        }
    }
}