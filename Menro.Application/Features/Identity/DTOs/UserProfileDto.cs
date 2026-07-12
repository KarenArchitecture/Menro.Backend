namespace Menro.Application.Features.Identity.DTOs
{
    // UserProfileDto.cs
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; } = string.Empty;
        public bool HasPassword { get; set; }
    }
}
