namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantProfileDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public int RestaurantCategoryId { get; set; }
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
        public string BankAccountNumber { get; set; } = "";

        public string OpenTime { get; set; } = "";   // "12:00"
        public string CloseTime { get; set; } = "";

        public string? BannerImageUrl { get; set; }
        public string? ShopBannerImageUrl { get; set; }
        public string? LogoImageUrl { get; set; }

        // Subscription
        public string? SubscriptionType { get; set; }
        public int SubscriptionDaysLeft { get; set; }
    }
}
