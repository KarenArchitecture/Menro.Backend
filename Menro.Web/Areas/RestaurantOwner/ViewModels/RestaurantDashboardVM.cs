using Menro.Domain.Entities;

namespace Menro.Web.Areas.RestaurantOwner.ViewModels
{
    public class RestaurantDashboardVM
    {
        public string Name { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public string OwnerFullName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string? ShebaNumber { get; set; }
        public string? SubscriptionTitle { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        // اگر نیاز داری، مثلا برای نمایش لیست غذاها یا دسته‌بندی‌ها
        public List<FoodCategory> FoodCategories { get; set; } = new();
        public List<Food> Foods { get; set; } = new();
    }
}
