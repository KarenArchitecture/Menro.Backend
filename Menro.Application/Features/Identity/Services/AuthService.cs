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
        public async Task SendOtpAsync(string phoneNumber)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            //await _smsSender.SendAsync(phoneNumber, $"کد تایید شما: {code}");
            var otp = new Otp
            {
                PhoneNumber = phoneNumber,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(2),
                IsUsed = false
            };

            await _uow.Otp.AddAsync(otp);
            await _uow.SaveChangesAsync();
        }

        // verification
        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            try
            {
                var otp = await _uow.Otp.GetLatestUnexpiredAsync(phoneNumber);
                if (otp is null || otp.Code != code)
                    return false;

                otp.IsUsed = true;

                await _uow.Otp.UpdateAsync(otp);
                await _uow.SaveChangesAsync();

                await PhoneConfirmed(phoneNumber);

                return true;
            }
            catch (Exception)
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


        /*--- misc. ---*/

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

    }
}
