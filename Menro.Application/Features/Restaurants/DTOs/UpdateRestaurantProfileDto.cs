using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class UpdateRestaurantProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Slug { get; set; }
        public int RestaurantCategoryId { get; set; }
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string NationalCode { get; set; } = "";
        public string? ShebaNumber { get; set; }
        public string BankAccountNumber { get; set; } = "";
        public string OpenTime { get; set; } = "";
        public string CloseTime { get; set; } = "";

        // Location
        [Range(-90, 90, ErrorMessage = "عرض جغرافیایی نامعتبر است")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "طول جغرافیایی نامعتبر است")]
        public double? Longitude { get; set; }

        // Social / messenger IDs
        [MaxLength(50)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره واتساپ باید با فرمت 09123456789 باشد")]
        public string? WhatsAppId { get; set; }

        [MaxLength(50)]
        [RegularExpression(@"^@?[A-Za-z0-9_.]+$", ErrorMessage = "شناسه تلگرام نامعتبر است")]
        public string? TelegramId { get; set; }

        [MaxLength(50)]
        [RegularExpression(@"^@?[A-Za-z0-9_.]+$", ErrorMessage = "شناسه اینستاگرام نامعتبر است")]
        public string? InstagramId { get; set; }

        [MaxLength(50)]
        [RegularExpression(@"^@?[A-Za-z0-9_.]+$", ErrorMessage = "شناسه ایتا نامعتبر است")]
        public string? EitaaId { get; set; }

        [MaxLength(50)]
        [RegularExpression(@"^@?[A-Za-z0-9_.]+$", ErrorMessage = "شناسه بله نامعتبر است")]
        public string? BaleId { get; set; }
        public IFormFile? HomeBanner { get; set; }
        public IFormFile? ShopBanner { get; set; }
        public IFormFile? Logo { get; set; }
        public bool RemoveHomeBanner { get; set; }
        public bool RemoveShopBanner { get; set; }
        public bool RemoveLogo { get; set; }
    }
}
