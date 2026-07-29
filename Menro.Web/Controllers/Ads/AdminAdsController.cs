using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Features.Ads.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Menro.Web.Controllers.Ads
{
    [ApiController]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Owner}")]
    [Route("api/admin/ads")]
    public class AdminAdsController : ApiControllerBase
    {
        #region DI
        private readonly IRestaurantAdService _service;
        private readonly ICurrentUserService _currentUserService;

        public AdminAdsController(IRestaurantAdService service,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }
        #endregion

        [HttpPost("addAd")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> Create([FromForm] ReserveRestaurantAdDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest(new { message = "تصویر تبلیغ نباید خالی باشد" });

            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });
            dto.RestaurantId = restaurantId;

            try
            {
                bool result = await _service.CreateAsync(dto);
                if (!result)
                    return BadRequest(new { message = "رستوران مورد نظر یافت نشد" });

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /* ad Management by ADMIN */
        [HttpGet("pending")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetPendingAds()
        {
            var list = await _service.GetPendingAdsAsync();
            return Ok(list);
        }

        [HttpGet("history")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetHistory()
        {
            var list = await _service.GetHistoryAsync();
            return Ok(list);
        }


        [HttpPost("{id}/approve")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Approve(int id)
        {
            var success = await _service.ApproveAdAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "تبلیغ تایید شد." });
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Reject([FromBody] RejectAdDto dto)
        {
            var success = await _service.RejectAdAsync(dto);
            if (!success) return NotFound();
            return Ok(new { message = "تبلیغ رد شد." });
        }
    }

}