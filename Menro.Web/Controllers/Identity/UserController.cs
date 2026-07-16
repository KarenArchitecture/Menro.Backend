using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Identity.DTOs;
using Menro.Application.Features.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Identity
{
    [ApiController]
    [Authorize]
    [Route("api/user")]
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
            // NOTE: GetProfileAsync must populate profile.HasPassword (see
            // UserProfileDto) — e.g. via UserManager<User>.HasPasswordAsync,
            // so the client can tell "change password" apart from
            // "set password for the first time".

            // mount profile image
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

        // Sets a password for accounts that don't have one yet (e.g.
        // OTP-only accounts). Deliberately separate from
        // AuthController.ChangePassword, which requires and checks a
        // CurrentPassword — that check would always fail here since there
        // is no current password to verify against.
        //
        // NOTE: IUserService needs a SetPasswordAsync(userId, newPassword)
        // method that goes through something like
        // UserManager<User>.AddPasswordAsync (NOT ChangePasswordAsync) and
        // should itself refuse to run if the user already has a password —
        // that case belongs to /api/auth/change-password instead.
        [Authorize]
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            var userId = _currentUserService.GetUserId()!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest(new { message = "رمز عبور باید حداقل ۶ کاراکتر باشد." });

            var result = await _userService.SetPasswordAsync(userId, dto.NewPassword);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "رمز عبور با موفقیت تنظیم شد." });
        }
    }
}
