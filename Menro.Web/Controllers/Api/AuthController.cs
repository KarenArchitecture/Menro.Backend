using Microsoft.AspNetCore.Mvc;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Menro.Application.Services.Interfaces;
using Menro.Application.DTO.Auth;
using Menro.Application.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Menro.Application.SD;

namespace Menro.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(
            IAuthService authService,
            UserManager<User> userManager,
            SignInManager<User> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
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
            if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                return BadRequest(new { message = "کد تایید نامعتبر است." });

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
            if (user == null)
                return Ok(new { needsRegister = true });

            var roles = await _userManager.GetRolesAsync(user);
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

        [HttpPost("login-password")]
        public async Task<IActionResult> LoginWithPassword([FromBody] LoginPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "شماره تلفن و رمز عبور الزامی است." });

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
            if (user == null)
                return BadRequest(new { message = "کاربری با این شماره وجود ندارد." });

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return BadRequest(new { message = "رمز عبور نادرست است." });

            var roles = await _userManager.GetRolesAsync(user);
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


    }
}
