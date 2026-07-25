using Menro.Application.Common.SD;
using Menro.Application.Features.Users.DTOs;
using Menro.Application.Features.Users.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Users
{
    /// <summary>
    /// Endpoints consumed by adminUsersAxios (baseURL: /admin/users) and
    /// UserManagementSection.jsx.
    /// </summary>
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = SD.Role_Admin)] // adjust to match the actual admin role name in the system
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        public AdminUsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserListItemDto>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _userManagementService.GetUsersAsync(new UserQueryParameters
            {
                Search = search,
                Role = role,
                Page = page,
                PageSize = pageSize,
            });

            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            var roles = await _userManagementService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user is null)
                return NotFound(new { message = "کاربر مورد نظر یافت نشد." });

            return Ok(user);
        }

        [HttpPut("{id}/roles")]
        public async Task<ActionResult<UserDetailDto>> UpdateRoles(
            string id, [FromBody] UpdateUserRolesDto dto)
        {
            try
            {
                var updated = await _userManagementService.UpdateUserRolesAsync(
                    id, dto.Roles ?? new List<string>());
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}