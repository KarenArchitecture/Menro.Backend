using System.Globalization;

namespace Menro.Application.Helpers
{
    /// <summary>
    /// Converts UTC .NET DateTimes into a display-ready Persian (Jalali)
    /// date string with Persian digits, e.g. "۲۹ شهریور ۱۴۰۵".
    /// </summary>
    public static class PersianDateHelper
    {
        private static readonly string[] MonthNames =
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند",
        };

        private static readonly char[] PersianDigits =
            { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };

        private static readonly PersianCalendar Calendar = new();

        /// <summary>
        /// Formats a date as "{day} {monthName} {year}" using Persian digits,
        /// e.g. "۲۹ شهریور ۱۴۰۵". Pass the value as UTC; convert to local
        /// time before calling this if the site later needs timezone-aware
        /// display dates.
        /// </summary>
        public static string ToPersianDisplayDate(DateTime dateTime)
        {
            var day = Calendar.GetDayOfMonth(dateTime);
            var month = Calendar.GetMonth(dateTime);
            var year = Calendar.GetYear(dateTime);

            return $"{ToPersianDigits(day)} {MonthNames[month - 1]} {ToPersianDigits(year)}";
        }

        private static string ToPersianDigits(int number)
        {
            var digits = number
                .ToString(CultureInfo.InvariantCulture)
                .Select(c => PersianDigits[c - '0']);

            return new string(digits.ToArray());
        }
    }
}
