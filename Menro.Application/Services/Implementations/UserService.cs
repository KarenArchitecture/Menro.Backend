using Menro.Application.Common.Models;
using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Menro.Application.Common.SD.SD;


namespace Menro.Application.Services.Implementations
{
    /*
    * شرح وظایف:
    ثبت نام
    لاگین
    پیدا کردن کاربر
    ادیت کردن اطلاعات کاربری
    */
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<User> _passwordHasher;


        public UserService(IUnitOfWork uow, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IHttpContextAccessor httpContextAccessor, 
            IPasswordHasher<User> passwordHasher)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = passwordHasher;
        }
        public async Task<User> GetByIdAsync(string userId)
        {
            var user = await _uow.User.GetByIdAsync(userId);
            if (user is null)
                throw new InvalidOperationException("User not found");
            return user;
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            var user = await _uow.User.GetByEmailAsync(email);
            if (user is null)
                throw new InvalidOperationException("User not found");
            return user;

        }
        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            var user = await _uow.User.GetByPhoneNumberAsync(phoneNumber);
            return user;
        }

        public async Task<(bool IsSuccess, IdentityResult? Result, User? User)> RegisterUserAsync(string fullName, string email, string phoneNumber, string? password)
        {
            var existingUserByPhone = await _uow.User.GetByPhoneNumberAsync(phoneNumber);
            if (existingUserByPhone != null)
                return (false, null, null);

            if (!string.IsNullOrWhiteSpace(email) && await _userManager.FindByEmailAsync(email) is not null)
                return (false, null, null);

            var safeEmail = string.IsNullOrWhiteSpace(email)
                ? $"{phoneNumber}@menro.fake"
                : email;

            var user = new User
            {
                FullName = fullName,
                Email = safeEmail,
                PhoneNumber = phoneNumber,
                UserName = safeEmail
            };

            IdentityResult result;
            if (string.IsNullOrWhiteSpace(password))
                result = await _userManager.CreateAsync(user);
            else
                result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return (false, result, null);

            await _userManager.AddToRoleAsync(user, Role_Customer);

            return (true, result, user);
        }

        public async Task<List<string>> GetRolesAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            bool isCorrect = await _userManager.CheckPasswordAsync(user, password);
            return isCorrect;
        }
        public async Task<Result> ResetPasswordAsync(string phoneNumber, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
                return Result.Failure("رمز جدید و تکرار آن برابر نیست.");

            // از متد موجود برای یافتن کاربر استفاده کن تا منطق جستجو یکجا باشه
            var user = await GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
                return Result.Failure("کاربری با این شماره یافت نشد.");

            // تولید توکن ریست (سرور-side) و استفاده از ResetPasswordAsync تا ولیدیشن‌ها اجرا شوند
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var identityResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
                return Result.Failure(errors);
            }

            return Result.Success();
        }

    }
}
