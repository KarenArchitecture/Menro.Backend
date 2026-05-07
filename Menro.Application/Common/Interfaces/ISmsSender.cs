
namespace Menro.Application.Common.Interfaces
{
    public record SmsSendResult(bool IsSuccess, string? ProviderMessage, long? OutboxId);

    public interface ISmsSender
    {
        Task<SmsSendResult> SendOtpAsync(string phoneNumber, string message, CancellationToken ct = default);
    }
}
