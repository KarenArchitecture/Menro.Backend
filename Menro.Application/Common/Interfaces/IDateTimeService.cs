using System.Globalization;

namespace Menro.Application.Common.Interfaces
{
    public interface IDateTimeService
    {
        DateTime ConvertToGregorian(int year, int month, int day);
        string ConvertToPersian(DateTime date);
        public int GetPersianMonth(DateTime date);
        string GetPersianMonthName(DateTime date);
        public int GetPersianYear(DateTime date);
    }
}
