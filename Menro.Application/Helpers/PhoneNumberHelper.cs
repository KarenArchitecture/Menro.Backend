using System;
using System.Text.RegularExpressions;

namespace Menro.Application.Common.Helpers
{
    /// <summary>
    /// نرمال‌سازی شماره موبایل ایرانی بین دو فرمت:
    ///  - فرمت ذخیره‌سازی / لایه‌های داخلی (Domain, Application, DB): +989xxxxxxxxx  (E.164)
    ///  - فرمت کلاینت (فرم، نمایش، API Response): 09xxxxxxxxx
    ///
    /// قانون استفاده:
    ///  - قبل از هر Insert/Update در دیتابیس یا پاس دادن به Domain => ToStorageFormat
    ///  - قبل از برگردوندن به کلاینت (DTO/ViewModel) => ToClientFormat
    ///  - همیشه قبل از هرکدوم، IsValid رو چک کنید (یا بذارید ToStorageFormat خودش خطا بده)
    /// </summary>
    public static class PhoneNumberHelper
    {
        // ورودی‌های قابل قبول: 09123456789 | 9123456789 | 989123456789 | +989123456789 | 00989123456789
        private static readonly Regex IranMobileRegex =
            new(@"^(?:\+?98|0098|0)?9(\d{9})$", RegexOptions.Compiled);

        /// <summary>
        /// تبدیل هر فرمت ورودی معتبر به فرمت canonical ذخیره‌سازی: +989xxxxxxxxx
        /// </summary>
        public static string ToStorageFormat(string? rawPhoneNumber)
        {
            var core = ExtractCore(rawPhoneNumber);
            return $"+989{core}";
        }

        /// <summary>
        /// تبدیل فرمت ذخیره‌شده (+989xxxxxxxxx) یا هر فرمت دیگه به فرمت نمایشی کلاینت: 09xxxxxxxxx
        /// </summary>
        public static string ToClientFormat(string? rawPhoneNumber)
        {
            var core = ExtractCore(rawPhoneNumber);
            return $"09{core}";
        }

        /// <summary>
        /// اعتبارسنجی شماره موبایل ایرانی، مستقل از فرمت ورودی (بدون پرتاب Exception).
        /// </summary>
        public static bool IsValid(string? rawPhoneNumber)
        {
            if (string.IsNullOrWhiteSpace(rawPhoneNumber))
                return false;

            return IranMobileRegex.IsMatch(CleanInput(rawPhoneNumber));
        }

        /// <summary>
        /// حذف فاصله، خط تیره و پرانتز از ورودی خام کاربر.
        /// </summary>
        private static string CleanInput(string input)
        {
            return Regex.Replace(input.Trim(), @"[\s\-\(\)]", "");
        }

        /// <summary>
        /// هسته‌ی ۹ رقمی شماره (بدون رقم ۹ ابتدایی) را بعد از اعتبارسنجی برمی‌گرداند.
        /// اگر فرمت نامعتبر باشد Exception پرتاب می‌کند — بهتره خطا زودهنگام بگیریم
        /// تا داده‌ی خراب وارد دیتابیس نشه.
        /// </summary>
        private static string ExtractCore(string? rawPhoneNumber)
        {
            if (string.IsNullOrWhiteSpace(rawPhoneNumber))
                throw new ArgumentException("شماره موبایل نمی‌تواند خالی باشد.", nameof(rawPhoneNumber));

            var cleaned = CleanInput(rawPhoneNumber);
            var match = IranMobileRegex.Match(cleaned);

            if (!match.Success)
                throw new ArgumentException($"فرمت شماره موبایل نامعتبر است: {rawPhoneNumber}", nameof(rawPhoneNumber));

            return match.Groups[1].Value;
        }
    }
}