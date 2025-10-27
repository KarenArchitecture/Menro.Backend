
namespace Menro.Application.Common.Interfaces
{
    public interface ISmsSender
    {
        Task SendAsync(string phoneNumber, string message);
    }
}
