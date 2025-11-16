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
        private readonly IFileService _fileService;
        public RestaurantAdsController(IRestaurantAdService service,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _service = service;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }


        [HttpPost("addAd")]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantAdDto dto)
        {
            if(dto.ImageFileName == null)
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

        [HttpPost("upload-ad-image")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> UploadAdImage([FromForm] IFormFile file)
        {
            string fileName = await _fileService.UploadAdImageAsync(file);

            return Ok(fileName);
        }
    }

}