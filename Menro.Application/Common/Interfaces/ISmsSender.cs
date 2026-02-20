
namespace Menro.Application.Common.Interfaces
{
    public interface ISmsSender
    {
        Task<bool> SendOtpAsync(string phoneNumber, string otp);
    }
}
