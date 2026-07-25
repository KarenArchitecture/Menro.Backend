using Microsoft.AspNetCore.Mvc;
using Menro.Application.Features.Identity.DTOs;
using Menro.Application.Features.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Users.Services.Interfaces;

namespace Menro.Web.Controllers.Identity
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IAuthService authService, IUserService userService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _userService = userService;
            _currentUserService = currentUserService;
        }

        // -------- helper: صدور توکن + کوکی رفرش (یک‌جا، برای هر سه عملیاتی که لاگین می‌کنن) --------
        private async Task<string> IssueSessionAsync(Domain.Entities.User user)
        {
            var roles = await _userService.GetRolesAsync(user);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
            var ua = Request.Headers["User-Agent"].ToString();

            var (accessToken, refreshToken, _, _) = await _authService.LoginAsync(user, roles, ip, ua);

            Response.Cookies.Append("menro.rtk", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // TODO: true بعد از https
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(20)
            });

            return accessToken;
        }

        // ============ 1) login with password ============
        [AllowAnonymous]
        [HttpPost("login/password")]
        public async Task<IActionResult> LoginWithPassword([FromBody] LoginPasswordDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            if (!await _authService.VerifyPasswordAsync(dto.PhoneNumber, dto.Password))
                return BadRequest(new { message = "شماره یا رمز عبور اشتباه است." });

            var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (user is null)
                return BadRequest(new { message = "کاربر یافت نشد." });

            var accessToken = await IssueSessionAsync(user);
            return Ok(new { accessToken, userId = user.Id });
        }

        // ============ 2) login with otp ============
        [AllowAnonymous]
        [HttpPost("login/otp")]
        public async Task<IActionResult> LoginWithOtp([FromBody] LoginOtpDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                return BadRequest(new { message = "کد وارد شده معتبر نیست." });

            var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (user is null)
            {
                // OTP همین الان تأیید شد؛ همون تأیید رو به یک ticket تبدیل
                // می‌کنیم تا کاربر مجبور نشه برای register دوباره کد بگیره.
                var registrationTicket = _authService.GenerateRegistrationTicket(dto.PhoneNumber);
                return Ok(new { needsRegister = true, registrationTicket });
            }

            var accessToken = await IssueSessionAsync(user);
            return Ok(new { accessToken, userId = user.Id });
        }

        // ============ 3) register ============
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            // 🔒 بدون این ticket، ثبت‌نام اصلاً شروع نمی‌شه — دیگه هیچ‌کس
            // نمی‌تونه با یک شماره‌ی دلخواه (حتی متعلق به شخص دیگه)
            // مستقیم به این endpoint درخواست بزنه.
            if (!_authService.ValidateRegistrationTicket(dto.RegistrationTicket, dto.PhoneNumber, out var ticketError))
                return BadRequest(new { message = ticketError });

            var (isSuccess, result, user) = await _userService.RegisterUserAsync(
                dto.FullName, dto.Email, dto.PhoneNumber, dto.Password);

            if (!isSuccess || user == null || result == null || !result.Succeeded)
            {
                var errors = result?.Errors?.Select(e => e.Description).ToList() ?? new List<string>();
                return BadRequest(new { message = "ثبت‌نام ناموفق بود.", errors });
            }

            // شماره از قبل داخل VerifyOtpAsync تأیید(confirm) شده — نیازی به
            // فراخوانی دستی PhoneConfirmed نیست.

            var accessToken = await IssueSessionAsync(user);
            var roles = await _userService.GetRolesAsync(user);

            return Ok(new
            {
                accessToken,
                user = new { user.Id, user.FullName, user.Email, user.PhoneNumber, Roles = roles }
            });
        }

        // ============ 4) change phone ============
        [Authorize]
        [HttpPut("change-phone")]
        public async Task<IActionResult> ChangePhone([FromBody] ChangePhoneRequestDto dto)
        {
            var userId = _currentUserService.GetUserId();
            dto.NewPhone = NormalizePhoneNumber(dto.NewPhone);

            // verify + commit در یک request؛ بدون فاصله‌ی زمانی برای race condition
            if (!await _authService.VerifyOtpAsync(dto.NewPhone, dto.Code))
                return BadRequest(new { message = "کد وارد شده معتبر نیست." });

            var takenByOther = await _userService.GetByPhoneNumberAsync(dto.NewPhone);
            if (takenByOther is not null && takenByOther.Id != userId)
                return BadRequest(new { message = "این شماره قبلاً در سیستم ثبت شده است." });

            var result = await _authService.ChangePhoneAsync(userId!, dto.NewPhone);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "شماره تلفن با موفقیت تغییر کرد." });
        }

        // ============ 5) change password ============
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            string? userId = _currentUserService.GetUserId();
            if (dto == null) return BadRequest();
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "رمز جدید و تکرار آن برابر نیست" });
            if (userId is null)
                return BadRequest(new { message = "کاربر یافت نشد" });

            var result = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "رمز عبور با موفقیت تغییر کرد." });
        }

        // ============ 6) forgot password — دو مرحله (verify سپس reset) ============
        [AllowAnonymous]
        [HttpPost("forgot-password/verify")]
        public async Task<IActionResult> VerifyForgotPassword([FromBody] ForgotPasswordVerifyDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                return BadRequest(new { message = "کد وارد شده معتبر نیست." });

            var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (user is null)
                return BadRequest(new { message = "کاربری با این شماره یافت نشد." });

            var resetToken = _authService.GeneratePasswordResetToken(dto.PhoneNumber);
            return Ok(new { resetToken });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null) return BadRequest();
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            if (string.IsNullOrWhiteSpace(dto.ResetToken))
                return BadRequest(new { message = "ابتدا باید شماره تلفن با کد تأیید احراز شود." });

            if (!_authService.ValidatePasswordResetToken(dto.ResetToken, dto.PhoneNumber, out var tokenError))
                return BadRequest(new { message = tokenError });

            if (dto.NewPassword != dto.NewPasswordConfirm)
                return BadRequest(new { message = "رمز جدید و تکرار آن برابر نیست" });

            var result = await _authService.ResetPasswordAsync(dto.PhoneNumber, dto.NewPassword);
            if (!result.IsSuccess)
                return BadRequest(new { message = "عملیات ناموفق" });

            return Ok(new { message = "رمز عبور با موفقیت تغییر کرد." });
        }

        // ============ 7) confirm phoneNumber (standalone — مثلاً برای وقتی
        // شماره‌ی جدید بدون رفتن به مسیر لاگین باید تأیید بشه) ============
        // !!! no use case for now !!!
        [AllowAnonymous]
        [HttpPost("confirm-phone")]
        public async Task<IActionResult> ConfirmPhone([FromBody] ConfirmPhoneDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            if (!await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.Code))
                return BadRequest(new { message = "کد وارد شده معتبر نیست." });

            var existingUser = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (existingUser is not null)
                return Ok(new { userExists = true }); // قبلاً حساب داره → باید بره لاگین

            var registrationTicket = _authService.GenerateRegistrationTicket(dto.PhoneNumber);
            return Ok(new { registrationTicket });
        }

        // ============ 8) refresh access token ============
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("menro.rtk", out var rawRt))
                return Unauthorized(new { message = "رفرش‌توکن پیدا نشد." });

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
                var ua = Request.Headers["User-Agent"].ToString();

                var (newAccess, newRefresh) = await _authService.RefreshAccessTokenAsync(rawRt, ip, ua);

                Response.Cookies.Append("menro.rtk", newRefresh, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(20)
                });

                return Ok(new { AccessToken = newAccess });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "رفرش‌توکن معتبر نیست یا منقضی شده." });
            }
        }

        // -------- بدون تغییر: send-otp / logout / me --------
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
            var result = await _authService.SendOtpAsync(dto.PhoneNumber);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error });
            return Ok(new { message = "کد تأیید ارسال شد." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("menro.rtk", out var rawRt))
                return Ok(new { message = "No active session found." });
            await _authService.LogoutAsync(rawRt);
            Response.Cookies.Delete("menro.rtk", new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict });
            return Ok(new { message = "خروج با موفقیت انجام شد." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "شناسه کاربر در توکن یافت نشد." });

            var user = await _userService.GetByIdAsync(userIdClaim);
            if (user is null) return NotFound(new { message = "کاربر یافت نشد." });

            var roles = await _userService.GetRolesAsync(user);
            return Ok(new { user.Id, user.FullName, user.Email, user.PhoneNumber, Roles = roles });
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;
            var p = phoneNumber.Trim().Replace(" ", "").Replace("-", "");
            return p.StartsWith("+98") ? p :
                   p.StartsWith("0098") ? "+" + p[2..] :
                   p.StartsWith("98") ? "+" + p :
                   p.StartsWith("0") && p.Length == 11 ? "+98" + p[1..] :
                   p.Length == 10 && p.StartsWith("9") ? "+98" + p :
                   phoneNumber;
        }
    }
}