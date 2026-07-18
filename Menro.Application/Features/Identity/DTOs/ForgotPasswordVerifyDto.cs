namespace Menro.Application.Features.Identity.DTOs
{
    public class ForgotPasswordVerifyDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
