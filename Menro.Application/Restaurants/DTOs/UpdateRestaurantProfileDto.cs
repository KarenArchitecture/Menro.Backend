using Microsoft.AspNetCore.Http;

namespace Menro.Application.Restaurants.DTOs
{
    public class UpdateRestaurantProfileDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public int RestaurantCategoryId { get; set; }
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string BankAccountNumber { get; set; } = "";

        public string OpenTime { get; set; } = "";
        public string CloseTime { get; set; } = "";

        public IFormFile? HomeBanner { get; set; }
        public IFormFile? ShopBanner { get; set; }
        public IFormFile? Logo { get; set; }

        public string? HomeBannerFileName { get; set; }
        public string? ShopBannerFileName { get; set; }
        public string? LogoFileName { get; set; }
    }
}
