namespace Menro.Application.Features.AdminPanel.DTOs
{
    public class SalesByMonthDto
    {
        public int Month { get; set; }          // 1..12 (میلادی)
        public decimal TotalSales { get; set; } // مجموع مبلغ فروش در آن ماه
    }
}
