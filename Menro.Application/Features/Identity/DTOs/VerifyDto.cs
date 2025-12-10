namespace Menro.Application.Features.Identity.DTOs
{
    public class VerifyDto
    {
        public string PhoneNumber { get; set; }
        public string Method { get; set; } // "otp" or "password"
        public string? Operation { get; set; } // login, change-phone, reset-pass, ...

        public string CodeOrPassword { get; set; }
    }
}
