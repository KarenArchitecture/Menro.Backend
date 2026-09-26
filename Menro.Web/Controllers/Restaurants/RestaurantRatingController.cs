using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Route("api/restaurant/rating")]
    [Authorize] // فقط کاربر لاگین‌شده اجازه‌ی رای دادن داره
    public class RestaurantRatingController : ApiControllerBase
    {
        private readonly IRestaurantRatingService _restaurantRatingService;
        private readonly ICurrentUserService _currentUserService;

        public RestaurantRatingController(
            IRestaurantRatingService restaurantRatingService,
            ICurrentUserService currentUserService)
        {
            _restaurantRatingService = restaurantRatingService;
            _currentUserService = currentUserService;
        }

        // POST: /api/restaurant/rating
        [HttpPost]
        public async Task<IActionResult> Rate([FromBody] RateRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.GetUserId();
            var result = await _restaurantRatingService.RateRestaurantAsync(userId, dto);

            return FromResult(result, "امتیاز شما با موفقیت ثبت شد.");
        }

        // GET: /api/restaurant/rating/{restaurantId}
        [HttpGet("{restaurantId:int}")]
        public async Task<IActionResult> GetMyRating(int restaurantId)
        {
            var userId = _currentUserService.GetUserId();
            var score = await _restaurantRatingService.GetUserRatingAsync(userId, restaurantId);
            return Ok(new { score });
        }
    }
}