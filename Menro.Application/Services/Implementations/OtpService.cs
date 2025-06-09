using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System.Security.Cryptography;

namespace Menro.Application.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly ISmsSender _smsSender;
        private readonly IUnitOfWork _uow;
        public OtpService(IUnitOfWork uow, ISmsSender smsSender)
        {
            _uow = uow;
            _smsSender = smsSender;
        }
        public async Task SendOtpAsync(string phoneNumber)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            await _smsSender.SendAsync(phoneNumber, $"کد تایید شما: {code}");
            var otp = new Otp
            {
                PhoneNumber = phoneNumber,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(2),
                IsUsed = false
            };

            await _uow.Otp.AddAsync(otp);
            await _uow.SaveAsync();

        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            var otp = await _uow.Otp.GetLatestUnexpiredAsync(phoneNumber);

            // شاید بهتر باشه این قابلیت رو هم اضافه کنیم که بخاطر هر ارور یک خروجی متفاوت بده (استفاده از int بعنوان خروجی؟!)
            if (otp == null || otp.Code != code)
                return false;

            otp.IsUsed = true;
            await _uow.Otp.UpdateAsync(otp);
            await _uow.SaveAsync();

            return true;
        }

    }
}
