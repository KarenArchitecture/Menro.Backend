using Menro.Application.Features.Users.DTOs;

namespace Menro.Application.Features.Users.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<PagedResult<UserListItemDto>> GetUsersAsync(UserQueryParameters query);

        Task<List<string>> GetRolesAsync();

        Task<UserDetailDto?> GetUserByIdAsync(string id);

        Task<UserDetailDto> UpdateUserRolesAsync(string id, List<string> roles);
    }
}