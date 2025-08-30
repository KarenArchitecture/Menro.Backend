using Microsoft.AspNetCore.Mvc;
using Menro.Application.Features.Identity.DTOs;
using System.Reflection.Metadata.Ecma335;
using Menro.Application.Features.Identity.Services;

namespace Menro.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(
            IAuthService authService,
            IUserService userService
            )
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            await _authService.SendOtpAsync(dto.PhoneNumber);
            return Ok(new { message = "کد تأیید ارسال شد." });

        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> LoginWithOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                    return BadRequest(new { message = "کد تایید نامعتبر است." });

                /*error*/ var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
                if (user is null)
                    return Ok(new { needsRegister = true });


                var roles = await _userService.GetRolesAsync(user);
                var token = _authService.GenerateToken(
                    Guid.Parse(user.Id),
                    user.UserName ?? "",
                    user.Email ?? "",
                    roles.ToList()
                );
                await _authService.PhoneConfirmed(dto.PhoneNumber);

                return Ok(new
                {
                    needsRegister = false,
                    token,
                    user = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.PhoneNumber,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
             {
                return StatusCode(500, new
                {
                    message = "Server error",
                    error = ex.Message,
                    stack = ex.StackTrace // (اختیاری برای دیدن خط کد دقیق)
                });
            }
        }
        [HttpPost("login-password")]
        public async Task<IActionResult> LoginWithPassword([FromBody] LoginPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "شماره تلفن و رمز عبور الزامی است." });

            try
            {
                var (token, user, roles) = await _authService.LoginWithPasswordAsync(dto.PhoneNumber, dto.Password);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.PhoneNumber,
                        Roles = roles
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (isSuccess, result, user) = await _userService.RegisterUserAsync(
                dto.FullName,
                dto.Email,
                dto.PhoneNumber,
                dto.Password
            );

            if (!isSuccess || user == null || result == null || !result.Succeeded)
            {
                var errors = result?.Errors?.Select(e => e.Description).ToList() ?? new List<string>();
                return BadRequest(new { message = "ثبت‌نام ناموفق بود.", errors });
            }

            await _authService.PhoneConfirmed(dto.PhoneNumber);

            var roles = await _userService.GetRolesAsync(user);
            var token = _authService.GenerateToken(
                Guid.Parse(user.Id),
                user.FullName ?? "",
                user.Email ?? "",
                roles.ToList()
            );

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    Roles = roles
                }
            });
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto == null) return BadRequest();

            var result = await _authService.ResetPasswordAsync(dto.PhoneNumber, dto.NewPassword, dto.NewPasswordConfirm);

            if (!result.IsSuccess)
                return BadRequest(new { message = "عملیات ناموفق" });

            return Ok(new { message = "رمز عبور با موفقیت تغییر کرد." });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // اگر از کوکی استفاده کنیم:
            // await _signInManager.SignOutAsync();

            // اگه بخوایم در آینده logout tokens یا blacklist رو ثبت کنیم اینجا انجام میدیم

            return Ok(new { message = "Logged out successfully" });
        }

    }
}
