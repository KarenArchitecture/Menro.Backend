namespace Menro.Application.Features.AdminPanel.DTOs
{
    public class SalesByMonthDto
    {
        public int Month { get; set; }          // 1..12 (میلادی)
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; } // مجموع مبلغ فروش در آن ماه
    }
}
