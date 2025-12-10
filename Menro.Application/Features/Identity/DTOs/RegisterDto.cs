namespace Menro.Application.Features.Identity.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Password { get; set; } // Optional
        public string PhoneNumber { get; set; }
        public string Email { get; set; } // Optional
    }
}
