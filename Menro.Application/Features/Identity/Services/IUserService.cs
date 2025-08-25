using Menro.Application.Common.Models;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Identity.Services
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<(bool IsSuccess, IdentityResult? Result, User? User)> RegisterUserAsync(string fullName, string email, string phoneNumber, string? password);
        //public Task<User?> LoginUserAsync(string email, string password);
        Task<List<string>> GetRolesAsync(User user);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword, string confirmPassword);
        Task<bool> AddRoleToUserAsync(string userId, string roleName);

    }
}
