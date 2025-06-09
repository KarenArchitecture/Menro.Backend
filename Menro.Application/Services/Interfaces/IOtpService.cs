namespace Menro.Application.Services.Interfaces
{
    public interface IOtpService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
    }
}
