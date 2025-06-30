using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Menro.Application.Services.Implementations;
using Menro.Application.Authentication.DTOs;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(
            JwtService jwtService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
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

    }
}
