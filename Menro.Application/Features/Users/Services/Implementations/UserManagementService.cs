using System.Linq;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Users.DTOs;
using Menro.Application.Features.Users.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Menro.Application.Features.Users.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        #region DI
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMediaStorageProvider _mediaStorage;

        public UserManagementService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            IMediaStorageProvider mediaStorage)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserQueryParameters query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            var (items, totalCount) = await _userRepository.SearchUsersAsync(
                query.Search, query.Role, page, pageSize);

            var rolesByUserId = await _userRepository.GetRolesForUserIdsAsync(
                items.Select(u => u.Id));

            var dtoItems = items
                .Select(u => MapToListItem(
                    u,
                    rolesByUserId.TryGetValue(u.Id, out var roles) ? roles : new List<string>()))
                .ToList();

            return new PagedResult<UserListItemDto>
            {
                Items = dtoItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        public async Task<List<string>> GetRolesAsync()
        {
            return await _userRepository.GetAllRoleNamesAsync();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(id);
            if (user is null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDetail(user, roles);
        }

        public async Task<List<string>> UpdateUserRolesAsync(string id, List<string> roles)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(id);
            if (user is null)
                throw new InvalidOperationException("کاربر مورد نظر یافت نشد.");

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToAdd = roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(roles).ToList();

            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", removeResult.Errors.Select(e => e.Description)));
            }

            if (rolesToAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(" ", addResult.Errors.Select(e => e.Description)));
            }

            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        private UserListItemDto MapToListItem(User user, IList<string> roles) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            ProfileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImage) 
            ? null 
            : _mediaStorage.GetUrl(MediaCategory.UserProfileImage, user.ProfileImage, entityId: user.Id, variant: MediaVariant.Thumbnail),
            PhoneNumber = NormalizePhoneNumber(user.PhoneNumber),
            Roles = roles.ToList(),
        };

        private UserDetailDto MapToDetail(User user, IList<string> roles) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            ProfileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImage)
            ? null 
            : _mediaStorage.GetUrl(MediaCategory.UserProfileImage, user.ProfileImage, entityId: user.Id, variant: MediaVariant.Resized),
            Email = user.Email,
            PhoneNumber = NormalizePhoneNumber(user.PhoneNumber),
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            Roles = roles.ToList(),
            RestaurantsCount = user.Restaurants?.Count ?? 0,
            OrdersCount = user.Orders?.Count ?? 0,
            FavoriteFoodsCount = user.FavoriteFoods?.Count ?? 0,
        };

        private static string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var p = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

            if (p.StartsWith("+98")) return "0" + p[3..];
            if (p.StartsWith("0098")) return "0" + p[4..];
            if (p.StartsWith("98")) return "0" + p[2..];
            if (p.StartsWith("0")) return p;
            if (p.StartsWith("9")) return "0" + p;

            return phoneNumber;
        }
    }
}