namespace Menro.Application.Features.Users.DTOs
{
    // Used only for accounts that don't have a password yet (e.g. OTP-only
    // accounts). Deliberately has no CurrentPassword field — unlike
    // ChangePasswordDto, this is the "there's nothing to confirm against"
    // path. See UserController.SetPassword.
    public class SetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
