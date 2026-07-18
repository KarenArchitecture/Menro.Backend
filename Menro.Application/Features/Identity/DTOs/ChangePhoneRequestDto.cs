namespace Menro.Application.Features.Identity.DTOs
{
    public class ChangePhoneRequestDto
    {
        public string NewPhone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // 👈 جدید: verify + commit با هم
    }
}
