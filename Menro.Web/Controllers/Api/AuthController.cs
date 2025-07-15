using Microsoft.AspNetCore.Mvc;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Menro.Application.Services.Interfaces;
using Menro.Application.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using static Menro.Application.Common.SD.SD;
using Menro.Application.Authentication.DTOs;

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
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest("شماره تلفن نامعتبر است.");

            await _authService.SendOtpAsync(dto.PhoneNumber);
            return Ok(new { message = "کد تأیید ارسال شد." });

        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                    return BadRequest(new { message = "کد تایید نامعتبر است." });

                /*error*/ var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
                if (user == null)
                    return Ok(new { needsRegister = true });

                var roles = await _userService.GetRolesAsync(user);
                var token = _authService.GenerateToken(
                    Guid.Parse(user.Id),
                    user.UserName ?? "",
                    user.Email ?? "",
                    roles.ToList()
                );

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
