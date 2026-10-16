namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantProfileDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public int RestaurantCategoryId { get; set; }
        public string? Slug { get; set; }
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
        public string NationalCode { get; set; } = "";
        public string? ShebaNumber { get; set; }
        public string BankAccountNumber { get; set; } = "";

        public string OpenTime { get; set; } = "";   // "12:00"
        public string CloseTime { get; set; } = "";

        // Location
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Social / messenger IDs (خام، نه URL)
        public string? WhatsAppId { get; set; }
        public string? TelegramId { get; set; }
        public string? InstagramId { get; set; }
        public string? EitaaId { get; set; }
        public string? BaleId { get; set; }

        // Images
        public string? BannerImageUrl { get; set; }
        public string? ShopBannerImageUrl { get; set; }
        public string? LogoImageUrl { get; set; }

        // Subscription
        public string? SubscriptionType { get; set; }
        public int SubscriptionDaysLeft { get; set; }
    }
}
