using Menro.Application.Common.Models;
using Menro.Domain.Entities;
using Menro.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Menro.Application.Features.Users.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(string id);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> UserExistsByPhoneAsync(string phoneNumber);
        Task<bool> UpdatePhoneNumberAsync(string userId, string newPhone);
        Task<(bool IsSuccess, IdentityResult? Result, User? User)> RegisterUserAsync(string phoneNumber);

        Task<List<string>> GetRolesAsync(User user);
        Task<Result> SetPasswordAsync(string userId, string newPassword);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword);
        Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> AddRoleToUserAsync(string userId, string roleName);

        /*--- user details ---*/
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
    }
}