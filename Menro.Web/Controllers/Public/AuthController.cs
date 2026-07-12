using Microsoft.AspNetCore.Mvc;
using Menro.Application.Features.Identity.DTOs;
using Menro.Application.Features.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Menro.Application.Common.Interfaces;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        #region DI
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            ICurrentUserService currentUserService
            )
        {
            _authService = authService;
            _userService = userService;
            _currentUserService = currentUserService;
        }

        #endregion

        // ✅
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

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
            await _authService.PhoneConfirmed(dto.PhoneNumber);

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

        // ✅
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
            var method = dto.Method?.ToLower();
            bool isValid = false;

            switch (method)
            {
                case "otp":
                    isValid = await _authService.VerifyOtpAsync(dto.PhoneNumber, dto.CodeOrPassword);
                    break;

                case "password":
                    isValid = await _authService.VerifyPasswordAsync(dto.PhoneNumber, dto.CodeOrPassword);
                    break;

                default:
                    return BadRequest(new { message = "method باید یکی از otp یا password باشد." });
            }

            if (!isValid)
                return BadRequest(new { message = "اعتبارسنجی ناموفق بود." });

            // change phone
            // NOTE: this endpoint has no [Authorize] attribute, but
            // _currentUserService.GetUserId() still resolves correctly as
            // long as UseAuthentication() runs globally and the frontend
            // sends the bearer token (authAxios does) — the JWT handler
            // populates HttpContext.User regardless of [Authorize].
            if (dto.Operation == "change-phone")
            {
                var currentUserId = _currentUserService.GetUserId();
                var existingUser = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);

                if (existingUser is null || existingUser.Id != currentUserId)
                    return Ok(new { needsRegister = true });

                return Ok(new { verified = true });
            }

            // authentication
            var user = await _userService.GetByPhoneNumberAsync(dto.PhoneNumber);

            if (user is null)
                return Ok(new { needsRegister = true });

            // 🔒 SECURITY FIX: mint a short-lived, phone-bound reset token
            // whenever an OTP was the thing that got verified here. This is
            // the proof-of-ownership artifact /reset-password now requires.
            //
            // Previously /reset-password only checked [Authorize] (i.e. "is
            // *some* user logged in?") and then reset whatever PhoneNumber
            // was in the request body — it never checked that an OTP had
            // actually been verified for that specific number, and it never
            // checked that the logged-in user was that phone's owner. That
            // meant any authenticated user could reset any other account's
            // password just by knowing their phone number. Binding this
            // token to the phone number and requiring it on reset closes
            // that hole regardless of who (if anyone) is logged in.
            var resetToken = method == "otp"
                ? _authService.GeneratePasswordResetToken(dto.PhoneNumber)
                : null;

            return Ok(new
            {
                verified = true,
                userId = user.Id,
                resetToken,
            });
        }

        // ✅
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginWithIdDto dto)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
                var ua = Request.Headers["User-Agent"].ToString();

                var user = await _userService.GetByIdAsync(dto.UserId);
                if (user == null)
                    return Unauthorized(new { message = "کاربر یافت نشد." });

                var roles = await _userService.GetRolesAsync(user);

                var (accessToken, refreshToken, _, _) =
                    await _authService.LoginAsync(user, roles, ip, ua);

                Response.Cookies.Append("menro.rtk", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(20)
                });

                return Ok(new { accessToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        // ✅
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("menro.rtk", out var rawRt))
                return Ok(new { message = "No active session found." });

            await _authService.LogoutAsync(rawRt);

            Response.Cookies.Delete("menro.rtk", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new { message = "خروج با موفقیت انجام شد." });
        }

        /*--- helpers ----*/

        // ✅
        // call otp generation
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);

            var result = await _authService.SendOtpAsync(dto.PhoneNumber);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "کد تأیید ارسال شد." });
        }
        // ✅
        // refresh user access token
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

                // rotation → کوکی جدید جایگزین قبلی
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        // ✅
        // reset password (forgot-password flow)
        //
        // 🔒 SECURITY FIX: this used to be [Authorize] with no check that
        // the logged-in user actually owned `dto.PhoneNumber` — meaning any
        // authenticated user could silently take over any other account by
        // just posting that account's phone number + a new password here,
        // completely bypassing the OTP step shown in the UI.
        //
        // It's now [AllowAnonymous] (this flow is for people who are
        // logged OUT and forgot their password — requiring a session
        // defeats the point) and instead requires `dto.ResetToken`, the
        // short-lived token /verify only issues after a correct OTP check
        // for this exact phone number. No token bound to this phone → no
        // reset, no matter who is or isn't logged in.
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

        // ✅
        // change password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            string? userId = _currentUserService.GetUserId();

            // validations
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

        // ✅
        // change phone number
        [Authorize]
        [HttpPut("change-phone")]
        public async Task<IActionResult> ChangePhone([FromBody] ChangePhoneDto dto)
        {
            var userId = _currentUserService.GetUserId();
            dto.NewPhone = NormalizePhoneNumber(dto.NewPhone);
            var user = await _userService.GetByPhoneNumberAsync(dto.NewPhone);

            if (user is null)
                return BadRequest(new { message = "کاربری با این شماره یافت نشد" });

            var result = await _authService.ChangePhoneAsync(userId!, dto.NewPhone);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "شماره تلفن با موفقیت تغییر کرد." });
        }

        // ✅
        // check authentication
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "شناسه کاربر در توکن یافت نشد." });

                var user = await _userService.GetByIdAsync(userIdClaim);
                if (user is null)
                    return NotFound(new { message = "کاربر یافت نشد." });

                var roles = await _userService.GetRolesAsync(user);

                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Server error",
                    error = ex.Message
                });
            }
        }


        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var p = phoneNumber.Trim()
                .Replace(" ", "")
                .Replace("-", "");

            return p.StartsWith("+98") ? p :
                   p.StartsWith("0098") ? "+" + p[2..] :
                   p.StartsWith("98") ? "+" + p :
                   p.StartsWith("0") && p.Length == 11 ? "+98" + p[1..] :
                   p.Length == 10 && p.StartsWith("9") ? "+98" + p :
                   phoneNumber;
        }
    }
}

// Secure = false, => Secure = true,
// بعد از اینکه https رو وصل کردیم
