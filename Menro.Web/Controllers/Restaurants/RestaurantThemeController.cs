using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Restaurants
{
    [ApiController]
    [Route("api/restaurant/theme")]
    [Authorize]
    public class RestaurantThemeController : ApiControllerBase
    {
        private readonly IRestaurantThemeService _themeService;
        private readonly ICurrentUserService _currentUserService;

        public RestaurantThemeController(
            IRestaurantThemeService themeService,
            ICurrentUserService currentUserService)
        {
            _themeService = themeService;
            _currentUserService = currentUserService;
        }

        // GET: /api/restaurant/theme
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var themeId = await _themeService.GetThemeIdAsync(_currentUserService.GetUserId());
            return Ok(new { themeId });
        }

        // PUT: /api/restaurant/theme
        [HttpPut]
        public async Task<IActionResult> Save([FromBody] SaveThemeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _themeService.SaveThemeAsync(_currentUserService.GetUserId(), dto.ThemeId);
            return FromResult(result, "قالب رستوران ذخیره شد.");
        }
    }
}