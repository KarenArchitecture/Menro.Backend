using Menro.Application.Common.Interfaces;
using System.Globalization;

namespace Menro.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        private readonly PersianCalendar _persianCalendar = new();

        public DateTime ConvertToGregorian(int year, int month, int day)
        {
            return _persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        public string ConvertToPersian(DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):00}/{_persianCalendar.GetDayOfMonth(date):00}";
        }
        public int GetPersianMonth(DateTime date)
        {
            return _persianCalendar.GetMonth(date);
        }
        public int GetPersianYear(DateTime date)
        {
            return _persianCalendar.GetYear(date);
        }
        public string GetPersianMonthName(DateTime date)
        {
            var month = _persianCalendar.GetMonth(date);
            return month switch
            {
                1 => "فروردین",
                2 => "اردیبهشت",
                3 => "خرداد",
                4 => "تیر",
                5 => "مرداد",
                6 => "شهریور",
                7 => "مهر",
                8 => "آبان",
                9 => "آذر",
                10 => "دی",
                11 => "بهمن",
                12 => "اسفند",
                _ => "-"
            };
        }
    }
}
