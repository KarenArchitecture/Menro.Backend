namespace Menro.Application.Common.Settings
{
    public class SmsSettings
    {
        public string ApiKey { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;

        // شناسه قالب (Template) ثبت‌شده در پنل SMS.ir، مخصوص ارسال OTP
        // از طریق endpoint /v1/send/verify. از بخش «لیست قالب‌ها» در پنل به‌دست می‌آید.
        public int OtpTemplateId { get; set; }

        // نگه داشته شده برای سازگاری با کدهای قدیمی‌تر که از یک الگوی متنی
        // ثابت برای OTP استفاده می‌کردند؛ در پیاده‌سازی جدید verify استفاده نمی‌شود.
        public string? OtpPatternCode { get; set; }
    }
}