using Menro.Application.Services.Interfaces;
using Menro.Application.Common.Settings;
using Menro.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Menro.Domain.Entities;


namespace Menro.Application.Services.Implementations
{
    /*
    * شرح وظایف:
    ارسال و تایید OTP
    صدور توکن JWT
    تشخیص نیاز به ثبت نام یا ادامه ورود به سایت
    */
    public class AuthService : IAuthService
    {
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
        /* --- OTP services --- */
        // send otp
        public async Task SendOtpAsync(string phoneNumber)
        {
            var code = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
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

        // verify otp
        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            var otp = await _uow.Otp.GetLatestUnexpiredAsync(phoneNumber);

            // شاید بهتر باشه این قابلیت رو هم اضافه کنیم که بخاطر هر ارور یک خروجی متفاوت بده (استفاده از int بعنوان خروجی؟!)
            if (otp == null || otp.Code != code)
                return false;

            otp.IsUsed = true;
            await _uow.Otp.UpdateAsync(otp);
            await _uow.SaveChangesAsync();

            return true;
        }
        // token generation for user
        public string GenerateToken(Guid userId, string fullName, string email, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, fullName),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
        public async Task<(string Token, User User, List<string> Roles)> LoginWithPasswordAsync(string phoneNumber, string password)
        {
            var user = await _userService.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
                throw new UnauthorizedAccessException("کاربری با این شماره وجود ندارد.");

            var isPasswordValid = await _userService.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("رمز عبور نادرست است.");

            var roles = await _userService.GetRolesAsync(user);

            var token = GenerateToken(
                Guid.Parse(user.Id),
                user.FullName ?? "",
                user.Email ?? "",
                roles.ToList()
            );

            return (token, user, roles);
        }


    }
}
