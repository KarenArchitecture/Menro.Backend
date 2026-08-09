using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [AllowAnonymous] // مشتری ممکنه لاگین نباشه (سفارش مهمان)
    [Route("api/public/restaurants/{restaurantId:int}/tables")]
    public class PublicRestaurantTableController : ApiControllerBase
    {
        private readonly IRestaurantTableService _restaurantTableService;

        public PublicRestaurantTableController(IRestaurantTableService restaurantTableService)
        {
            _restaurantTableService = restaurantTableService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int restaurantId)
        {
            var tablesList = await _restaurantTableService.GetAllByRestaurantIdForPublicAsync(restaurantId);
            return Ok(tablesList);
        }
    }
}