namespace Menro.Application.Features.Identity.DTOs
{
    public class VerifyOtpDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
