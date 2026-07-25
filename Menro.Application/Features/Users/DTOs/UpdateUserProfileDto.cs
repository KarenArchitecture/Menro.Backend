using Microsoft.AspNetCore.Http;
namespace Menro.Application.Features.Users.DTOs
{
    public class UpdateUserProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
