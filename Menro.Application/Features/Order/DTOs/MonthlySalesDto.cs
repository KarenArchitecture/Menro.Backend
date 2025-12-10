namespace Menro.Application.Features.Order.DTOs
{
    public class MonthlySalesDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}
