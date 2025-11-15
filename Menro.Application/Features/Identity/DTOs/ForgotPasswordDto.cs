namespace Menro.Application.Features.Identity.DTOs
{
    public class ForgotPasswordDto
    {
        public string PhoneNumber { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }
}
