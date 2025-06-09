using Microsoft.AspNetCore.Mvc;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Menro.Application.Services.Interfaces;
using Menro.Application.DTO.Auth;
using Menro.Application.Services.Implementations;

namespace Menro.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IOtpService _otpService;

        public AuthController(
            IJwtService jwtService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOtpService otpService)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid login request");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(
                Guid.Parse(user.Id),
                user.UserName ?? "", // یا FullName اگه توی مدل User داری
                user.Email!,
                roles.ToList()
            );

            return Ok(new
            {
                token,
                expiresIn = 60 * 60, // اگر خواستی دقیق‌تر باشه می‌تونی از settings بگیری
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    Roles = roles
                }
            });
        }

        // تست نهایی نشده
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "این ایمیل قبلاً ثبت شده است." });

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserName = dto.Email,
                //EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "ثبت‌نام ناموفق بود.", errors });
            }

            // پیش‌فرض نقش مشتری
            await _userManager.AddToRoleAsync(user, "Customer");

            var roles = await _userManager.GetRolesAsync(user);
            if (!Guid.TryParse(user.Id, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var token = _jwtService.GenerateToken(
                userId,
                user.FullName ?? "",
                user.Email!,
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
                    Roles = roles
                }
            });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SenOtp([FromBody] SendOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest("شماره تلفن نامعتبر است.");

            await _otpService.SendOtpAsync(dto.PhoneNumber);
            return Ok(new { message = "کد تأیید ارسال شد." });

        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (await _otpService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
            {
                return Ok();
            }
            return BadRequest("معتبر نیست!");
        }
    }
}
