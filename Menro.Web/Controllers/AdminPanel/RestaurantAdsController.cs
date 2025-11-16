using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Features.Ads.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/ads")]
    public class RestaurantAdsController : ControllerBase
    {
        private readonly IRestaurantAdService _service;
        private readonly ICurrentUserService _currentUserService;
        public RestaurantAdsController(IRestaurantAdService service,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }


        [HttpPost("addAd")]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantAdDto dto)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });
            dto.RestaurantId = restaurantId;

            bool resutl = await _service.CreateAsync(dto);
            if (!resutl)
                return BadRequest(new { message = "عملیات ناموفق" });

            return Ok();
        }

        [HttpGet("{restaurantId}")]
        public async Task<IActionResult> GetList(int restaurantId)
        {
            var result = await _service.GetByRestaurantAsync(restaurantId);
            return Ok(result);
        }

        [HttpPost("ad-banner")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> UploadAdBanner([FromForm] IFormFile file)
        {

            return Ok();
        }
    }

}