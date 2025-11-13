using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Models;
using Menro.Application.Features.Identity.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using static Menro.Application.Common.SD.SD;




namespace Menro.Application.Features.Identity.Services
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
        private readonly IFileStorageService _fileStorage;

        public UserService(IUnitOfWork uow,
            UserManager<User> userManager,
            IFileStorageService fileStorage)
        {
            _uow = uow;
            _userManager = userManager;
            _fileStorage = fileStorage;
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
        public async Task<bool> AddRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /*--- user details ---*/
        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("کاربر یافت نشد");

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                ProfileImageUrl = user.ProfileImageUrl // just file name
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("کاربر یافت نشد");

                user.FullName = dto.FullName;

                if (dto.ProfileImage != null)
                    user.ProfileImageUrl = await _fileStorage.SaveProfileImageAsync(
                        dto.ProfileImage,
                        user.ProfileImageUrl 
                    );

                await _userManager.UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
