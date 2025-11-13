using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Identity.DTOs;
using Menro.Application.Features.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileUrlService _fileUrlService;

        public UserController(IUserService userService, ICurrentUserService currentUserService, IFileUrlService fileUrlService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _fileUrlService = fileUrlService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = _currentUserService.GetUserId()!;
            var profile = await _userService.GetProfileAsync(userId);

            if (!string.IsNullOrEmpty(profile.ProfileImageUrl))
                profile.ProfileImageUrl = _fileUrlService.BuildProfileImageUrl(profile.ProfileImageUrl);

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
        {
            var userId = _currentUserService.GetUserId()!;

            // file validation
            if (dto.ProfileImage != null)
            {
                var ext = Path.GetExtension(dto.ProfileImage.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowed.Contains(ext))
                    return BadRequest("فرمت فایل مجاز نیست (jpg, png, webp)");

                if (dto.ProfileImage.Length > 1_000_000)
                    return BadRequest("حجم فایل نباید بیش از 1 مگابایت باشد");
            }

            var updated = await _userService.UpdateProfileAsync(userId, dto);

            if (!updated)
                return StatusCode(500, new { message = "خطا در به‌روزرسانی پروفایل" });

            return Ok(new { message = "پروفایل با موفقیت به‌روزرسانی شد." });
        }
    }
}
