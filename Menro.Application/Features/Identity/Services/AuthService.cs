using Menro.Application.Common.Settings;
using Menro.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Menro.Domain.Entities;
using Menro.Application.Common.Models;
using Menro.Application.Common.Interfaces;
using Menro.Domain.Entities.Identity;



namespace Menro.Application.Features.Identity.Services
{
    /*
    * شرح وظایف:
    ارسال و تایید OTP
    صدور توکن JWT
    تشخیص نیاز به ثبت نام یا ادامه ورود به سایت
    */
    public class AuthService : IAuthService
    {
        #region DI
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwtSettings;
        private readonly ISmsSender _smsSender;

        // ⏱️ Minimum time that must pass between two OTP sends to the same
        // phone number. This is the single source of truth for the
        // rate limit — adjust here if the desired window changes.
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);

        // ⏱️ How long a password-reset token stays valid after OTP
        // verification. Short window: it only needs to survive the time
        // between finishing step 2 (enter OTP) and submitting step 3
        // (new password) of the forgot-password flow.
        private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromMinutes(10);

        private const string PasswordResetPurposeClaim = "purpose";
        private const string PasswordResetPurposeValue = "password-reset";
        private const string PasswordResetPhoneClaim = "phone";

        public AuthService(
            JwtSettings jwtSettings,
            IUnitOfWork uow,
            ISmsSender smsSender,
            IUserService userService)
        {
            _jwtSettings = jwtSettings;
            _uow = uow;
            _smsSender = smsSender;
            _userService = userService;
        }
        #endregion


        /*--- login management ---*/

        // send otp
        public async Task<Result> SendOtpAsync(string phoneNumber)
        {
            var phone = NormalizeIranMobileToE164(phoneNumber);
            var now = DateTime.UtcNow;

            // ⏱️ Rate limit: refuse to issue a new OTP if the last one sent
            // to this phone number is still within the cooldown window.
            // GetLatestUnexpiredAsync works for this because the OTP's own
            // expiration (2 minutes) is longer than the cooldown, so a
            // just-sent code is still "unexpired" when we check.
            var lastOtp = await _uow.Otp.GetLatestUnexpiredAsync(phone);
            if (lastOtp is not null)
            {
                var elapsed = now - lastOtp.CreatedAt;
                if (elapsed < OtpResendCooldown)
                {
                    var secondsLeft = (int)Math.Ceiling((OtpResendCooldown - elapsed).TotalSeconds);
                    return Result.Failure($"لطفاً {secondsLeft} ثانیه دیگر دوباره تلاش کنید.");
                }
            }

            // برای تست و Development
            var code = "12345";

            /* نسخه Production (ارسال واقعی SMS)
            var code = RandomNumberGenerator.GetInt32(10000, 100000).ToString();
            var send = await _smsSender.SendOtpAsync(phone, $"کد تایید شما: {code}");

            if (!send.IsSuccess)
                throw new Exception($"SMS failed: {send.ProviderMessage}");
            */

            await _uow.Otp.AddAsync(new Otp
            {
                PhoneNumber = phone,
                Code = ComputeHash(code),
                CreatedAt = now,
                ExpirationTime = now.AddMinutes(2),
                IsUsed = false
            });

            await _uow.SaveChangesAsync();

            return Result.Success();
        }
        // verify otp
        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            try
            {
                var phone = NormalizeIranMobileToE164(phoneNumber);
                var otp = await _uow.Otp.GetLatestUnexpiredAsync(phone);
                if (otp is null || otp.Code != ComputeHash(code))
                    return false;

                otp.IsUsed = true;
                await _uow.Otp.UpdateAsync(otp);
                await _uow.SaveChangesAsync();

                await PhoneConfirmed(phone);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyPasswordAsync(string phoneNumber, string password)
        {
            var user = await _userService.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
                return false;

            return await _userService.CheckPasswordAsync(user, password);
        }

        // login
        public async Task<(string AccessToken, string RefreshToken, User User, List<string> Roles)>
            LoginAsync(User user, IEnumerable<string> roles, string ip, string? userAgent)
        {
            var accessToken = GenerateToken(Guid.Parse(user.Id), user.FullName ?? "", user.Email ?? "", roles.ToList());
            var (rawRt, entity) = IssueRefreshToken(user.Id, ip, userAgent);

            await _uow.RefreshToken.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return (accessToken, rawRt, user, roles.ToList());
        }

        /*--- change password ---*/
        // for forgot-password
        public async Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword)
        {
            var result = await _userService.ResetPasswordAsync(phoneNumber, newPassword);
            return result;
        }
        // for change-password
        public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);
            return result;
        }
        /*------*/

        // change phone
        public async Task<Result> ChangePhoneAsync(string userId, string newPhone)
        {
            if (string.IsNullOrWhiteSpace(newPhone))
                return Result.Failure("شماره جدید وارد نشده است.");

            var exists = await _userService.UserExistsByPhoneAsync(newPhone);
            if (exists)
                return Result.Failure("این شماره تلفن قبلاً در سیستم ثبت شده است.");

            var updated = await _userService.UpdatePhoneNumberAsync(userId, newPhone);
            if (!updated)
                return Result.Failure("خطا در تغییر شماره.");

            await _uow.SaveChangesAsync();

            return Result.Success();
        }

        // logout
        public async Task<bool> LogoutAsync(string rawRefreshToken)
        {
            var hash = ComputeHash(rawRefreshToken);
            var stored = await _uow.RefreshToken.FindByHashAsync(hash);
            if (stored == null) return false;

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            await _uow.RefreshToken.UpdateAsync(stored);
            await _uow.SaveChangesAsync();
            return true;
        }


        /*--- jwt management ---*/

        // create Refresh Token
        public (string RawToken, RefreshToken Entity)
            IssueRefreshToken(string userId, string ip, string? userAgent)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); // 512bit random
            var tokenHash = ComputeHash(rawToken);

            var entity = new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(20),
                CreatedByIp = ip,
                UserAgent = userAgent,
                IsRevoked = false
            };

            return (rawToken, entity);
        }

        // refresh Access Token
        public async Task<(string NewAccessToken, string NewRefreshToken)>
            RefreshAccessTokenAsync(string rawRefreshToken, string ip, string? userAgent)
        {
            var hash = ComputeHash(rawRefreshToken);
            var stored = await _uow.RefreshToken.FindByHashAsync(hash);

            if (stored == null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("رفرش‌توکن معتبر نیست.");

            // rotation: invalidate old token
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;

            // build new refresh token
            var (newRaw, newEntity) = IssueRefreshToken(stored.UserId, ip, userAgent);
            stored.ReplacedByTokenHash = newEntity.TokenHash;

            await _uow.RefreshToken.AddAsync(newEntity);
            await _uow.SaveChangesAsync();

            // ساخت Access Token جدید
            var user = await _userService.GetByIdAsync(stored.UserId);
            var roles = await _userService.GetRolesAsync(user);
            var newAccess = GenerateToken(Guid.Parse(user.Id), user.FullName ?? "", user.Email ?? "", roles.ToList());


            return (newAccess, newRaw);
        }

        // generate jwt token (access token)
        public string GenerateToken(Guid userId, string fullName, string email, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, fullName),
                //new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔒 SECURITY FIX (new): mint a short-lived token that proves "an
        // OTP was just correctly verified for this exact phone number".
        // /reset-password requires this instead of trusting [Authorize] +
        // a phone number in the request body, which is what previously
        // let any logged-in user reset any other account's password.
        public string GeneratePasswordResetToken(string phoneNumber)
        {
            var claims = new List<Claim>
            {
                new Claim(PasswordResetPhoneClaim, phoneNumber),
                new Claim(PasswordResetPurposeClaim, PasswordResetPurposeValue),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(PasswordResetTokenLifetime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Validates a password-reset token AND that it was issued for the
        // exact phone number the caller is now trying to reset. Both checks
        // are required — a valid-but-mismatched-phone token must fail too,
        // otherwise someone could reset their own phone's password (to get
        // a legitimately-signed token) and then replay it against a
        // different phone number.
        public bool ValidatePasswordResetToken(string token, string expectedPhoneNumber, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                error = "توکن بازیابی رمز عبور یافت نشد.";
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = handler.ValidateToken(token, validationParameters, out _);

                var purpose = principal.FindFirst(PasswordResetPurposeClaim)?.Value;
                if (purpose != PasswordResetPurposeValue)
                {
                    error = "توکن برای این عملیات معتبر نیست.";
                    return false;
                }

                var tokenPhone = principal.FindFirst(PasswordResetPhoneClaim)?.Value;
                if (string.IsNullOrEmpty(tokenPhone) || tokenPhone != expectedPhoneNumber)
                {
                    error = "این توکن متعلق به این شماره تلفن نیست.";
                    return false;
                }

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                error = "نشست بازیابی رمز منقضی شده؛ لطفاً دوباره کد تأیید را دریافت کنید.";
                return false;
            }
            catch (Exception)
            {
                error = "توکن بازیابی رمز عبور نامعتبر است.";
                return false;
            }
        }


        /*--- utilities ---*/

        // hash token for save in db
        public static string ComputeHash(string input)
        {
            var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }

        // marks user phone number as verified
        public async Task<bool> PhoneConfirmed(string phoneNumber)
        {
            var user = await _userService.GetByPhoneNumberAsync(phoneNumber);
            if (user is null)
            {
                return false;
            }
            if (!user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = true;
                await _uow.User.UpdateAsync(user);
                await _uow.SaveChangesAsync();
            }
            return true;
        }

        // phonenumber normalize
        private static string NormalizeIranMobileToE164(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Exception("شماره موبایل الزامی است.");

            var p = phoneNumber.Trim().Replace(" ", "").Replace("-", "");
            return p.StartsWith("+98") ? p :
                   p.StartsWith("0098") ? "+" + p[2..] :
                   p.StartsWith("98") ? "+" + p :
                   p.StartsWith("0") && p.Length == 11 ? "+98" + p[1..] :
                   p.Length == 10 && p.StartsWith("9") ? "+98" + p :
                   throw new Exception("فرمت شماره موبایل معتبر نیست.");
        }
    }
}
