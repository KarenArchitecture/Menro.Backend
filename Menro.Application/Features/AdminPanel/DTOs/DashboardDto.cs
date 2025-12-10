namespace Menro.Application.Features.AdminPanel.DTOs
{
    public class DashboardDto
    {
        //public string RestaurantName { get; set; } = string.Empty;
        public int TodayOrdersCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SalesByMonthDto> MonthlySales { get; set; } = new();
    }
}
