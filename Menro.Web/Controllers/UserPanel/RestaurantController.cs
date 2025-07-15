using Menro.Application.DTO;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Menro.Web.Controllers.UserPanel
{
    [ApiController]
    [Authorize]
    [Route("api/user/restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IUserRecentOrderCardService _recentOrderCardService;

        public RestaurantController(IUserRecentOrderCardService recentOrderCardService)
        {
            _recentOrderCardService = recentOrderCardService;
        }

        // GET: api/user/restaurant/recent-orders
        [HttpGet("recent-orders")]
        public async Task<ActionResult<List<RestaurantCardDto>>> GetUserRecentOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recentRestaurants = await _recentOrderCardService.GetRecentOrderedRestaurantCardsAsync(userId);
            return Ok(recentRestaurants);
        }
    }
}
