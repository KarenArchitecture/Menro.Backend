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
    public class AdminAdsController : ControllerBase
    {
        #region DI

        private readonly IRestaurantAdService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;

        public AdminAdsController(IRestaurantAdService service,
            ICurrentUserService currentUserService,
            IFileService fileService,
            IFileUrlService fileUrlService)
        {
            _service = service;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _fileUrlService = fileUrlService;
        }

        #endregion

        /* ad reservation by OWNER */
        [HttpPost("upload-ad-image")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> UploadAdImage([FromForm] UploadAdImageDto dto)
        {
            string fileName = await _fileService.UploadAdImageAsync(dto.File);

            return Ok(fileName);
        }
        [HttpPost("addAd")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> Create([FromBody] ReserveRestaurantAdDto dto)
        {
            if (dto.ImageFileName == null)
                return BadRequest(new { message = "تصویر تبلیغ نباید خالی باشد" });

            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null)
                return BadRequest(new { message = "مشخصات رستوران متقاضی یافت نشد" });
            dto.RestaurantId = restaurantId;

            bool resutl = await _service.CreateAsync(dto);
            if (!resutl)
                return BadRequest(new { message = "عملیات ناموفق" });

            return Ok();
        }


        /* ad Management by ADMIN */
        [HttpGet("pending")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetPendingAds()
        {
            var list = await _service.GetPendingAdsAsync();
            list.ForEach(ad =>
            {
                ad.ImageUrl = _fileUrlService.BuildAdImageUrl(ad.ImageUrl);
            });

            return Ok(list);
        }

        [HttpGet("history")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> GetHistory()
        {
            var list = await _service.GetHistoryAsync();

            list.ForEach(ad =>
            {
                ad.ImageUrl = _fileUrlService.BuildAdImageUrl(ad.ImageUrl);
            });

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