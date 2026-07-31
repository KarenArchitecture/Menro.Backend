using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Ads.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Ads
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/restaurant-ads")]
    public class RestaurantAdsController : ApiControllerBase
    {
        private readonly IRestaurantAdService _service;
        private readonly ICurrentUserService _currentUserService;

        public RestaurantAdsController(
            IRestaurantAdService service,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });

            var list = await _service.GetMyPendingAdsAsync(restaurantId);
            return Ok(list);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });

            var list = await _service.GetMyHistoryAdsAsync(restaurantId);
            return Ok(list);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });

            var list = await _service.GetMyActiveAdsAsync(restaurantId);
            return Ok(list);
        }
    }
}