using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/owner/restaurant/tables")]
    public class RestaurantTableController : ApiControllerBase
    {
        #region DI
        private readonly ICurrentUserService _currentUserService;
        private readonly IRestaurantTableService _restaurantsTableService;

        public RestaurantTableController(
            IRestaurantTableService restaurantTableService,
            ICurrentUserService currentUserService)
        {
            _restaurantsTableService = restaurantTableService;
            _currentUserService = currentUserService;
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var tablesList = await _restaurantsTableService.GetAllByRestaurantIdAsync(restaurantId);
            return Ok(tablesList);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAsync(CreateRestaurantTableDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Label))
                return BadRequest(new { message = "برچسب میز الزامی است." });

            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _restaurantsTableService.AddTableAsync(dto, restaurantId);

            return FromResult(result, "میز با موفقیت اضافه شد.");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateRestaurantTableDto dto)
        {
            var result = await _restaurantsTableService.UpdateTableAsync(dto);
            return FromResult(result, "میز با موفقیت ویرایش شد.");
        }

        [HttpDelete("delete/{tableId}")]
        public async Task<IActionResult> DeleteAsync(int tableId)
        {
            var result = await _restaurantsTableService.DeleteTableAsync(tableId);
            if (!result)
                return BadRequest(new { message = "حذف میز موفق نبود." });

            return Ok(new { message = "میز حذف شد." });
        }
    }
}