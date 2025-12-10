using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/owner/restaurant")]
    [Authorize(Roles = SD.Role_Owner)]

    public class OwnerRestaurantController : ControllerBase
    {
        #region DI
        private readonly IRestaurantService _service;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;
        private readonly ICurrentUserService _currentUserService;

        public OwnerRestaurantController(IRestaurantService service,
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


        // restaurant profile
        [HttpGet("profile")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> GetProfile()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _service.GetRestaurantProfileAsync(restaurantId);
            if (result == null)
                return NotFound();

            // convert to URLs (ONLY if file name exists)
            if (!string.IsNullOrEmpty(result.BannerImageUrl))
                result.BannerImageUrl = _fileUrlService.BuildRestaurantHomeBannerUrl(result.BannerImageUrl);

            if (!string.IsNullOrEmpty(result.ShopBannerImageUrl))
                result.ShopBannerImageUrl = _fileUrlService.BuildRestaurantShopBannerUrl(result.ShopBannerImageUrl);

            if (!string.IsNullOrEmpty(result.LogoImageUrl))
                result.LogoImageUrl = _fileUrlService.BuildRestaurantLogoUrl(result.LogoImageUrl);

            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = SD.Role_Owner)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateRestaurantProfileDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            dto.Id = restaurantId;

            // 1) Get existing restaurant from database
            var restaurant = await _service.GetRestaurantByIdAsync(restaurantId);
            if (restaurant == null)
                return NotFound("Restaurant not found");

            // Keep old file names for deletion
            var oldHomeBanner = restaurant.BannerImageUrl;
            var oldShopBanner = restaurant.ShopBannerImageUrl;
            var oldLogo = restaurant.LogoImageUrl;

            // 2) Upload new files (FileService will delete old file if provided)
            if (dto.HomeBanner != null)
                dto.HomeBannerFileName =
                    await _fileService.UploadRestaurantHomeBannerAsync(dto.HomeBanner, oldHomeBanner);

            if (dto.ShopBanner != null)
                dto.ShopBannerFileName =
                    await _fileService.UploadRestaurantShopBannerAsync(dto.ShopBanner, oldShopBanner);

            if (dto.Logo != null)
                dto.LogoFileName =
                    await _fileService.UploadRestaurantLogoAsync(dto.Logo, oldLogo);

            // 3) Update database
            await _service.UpdateRestaurantProfileAsync(dto);

            return Ok(new { message = "Updated successfully" });
        }
    }
}
