using Menro.Application.Common.Interfaces;

namespace Menro.Infrastructure.Sms
{
    public class FakeSmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message)
        {
            Console.WriteLine($"[FAKE SMS] به {phoneNumber}: {message}");
            return Task.CompletedTask;
            //این کلاس صرفاً پیام رو در کنسول چاپ می‌کنه. برای تست محلی یا unit test عالیه
        }
    }
}
