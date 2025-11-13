using Menro.Application.Common.Models;
using Menro.Domain.Entities;
using Menro.Domain.Entities.Identity;

namespace Menro.Application.Features.Identity.Services
{
    public interface IAuthService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
        Task<bool> VerifyPasswordAsync(string phoneNumber, string password);
        Task<bool> PhoneConfirmed(string phoneNumber);
        Task<(string AccessToken, string RefreshToken, User User, List<string> Roles)>
                    LoginAsync(User user, IEnumerable<string> roles, string ip, string? userAgent);
        Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword, string confirmPassword);
        Task<Result> ChangePhoneAsync(string userId, string newPhone);
        string GenerateToken(Guid userId, string fullName, string email, List<string> roles);
        (string RawToken, RefreshToken Entity) IssueRefreshToken(string userId, string ip, string? userAgent);
        Task<(string NewAccessToken, string NewRefreshToken)> RefreshAccessTokenAsync(string rawRefreshToken, string ip, string? userAgent);
        Task<bool> LogoutAsync(string rawRefreshToken);


    }

}
