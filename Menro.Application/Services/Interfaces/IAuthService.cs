using Menro.Application.Common.Models;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
        Task<bool> PhoneConfirmed(string phoneNumber);
        string GenerateToken(Guid userId, string fullName, string email, List<string> roles);
        Task<(string Token, User User, List<string> Roles)> LoginWithPasswordAsync(string phoneNumber, string password);
        Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword, string confirmPassword);

    }

}
