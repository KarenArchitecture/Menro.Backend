using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class Restaurant : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "نام کامل")]
        [MaxLength(50)]
        [Required(ErrorMessage = "نام رستوران الزامی است")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "اسلاگ")]
        [MaxLength(100)]
        [Required(ErrorMessage = "اسلاگ الزامی است")]
        public string Slug { get; set; } = string.Empty;
        [Display(Name = "آدرس تصویر بنر صفحه خانه")]
        public string? BannerImageUrl { get; set; }
        [Display(Name = "آدرس تصویر بنر صفحه فروشگاه")]
        public string? ShopBannerImageUrl { get; set; }
        [Display(Name = "آدرس عکس بنر تبلیغاتی ")]
        public string? CarouselImageUrl { get; set; }
        [Display(Name = "آدرس لوگو")]
        public string? LogoImageUrl { get; set; }
        [Required(ErrorMessage = "افزودن آدرس رستوران الزامی است")]
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public DateTime CreatedAt { get; set; }
        [Display(Name = "توضیحات")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;
        [Display(Name = "دلیل رد")]
        [MaxLength(500)]
        public string? RejectReason { get; set; }
        // مشخصات صاحب رستوران
        [Display(Name = "کد ملی")]
        [MaxLength(10)]
        [Required(ErrorMessage = "کد ملی الزامی است")]
        public string NationalCode { get; set; } = string.Empty;
        [Display(Name = "شماره حساب")]
        [MaxLength(34)]
        public string BankAccountNumber { get; set; } = string.Empty;
        [Display(Name = "شماره شبا")]
        [MaxLength(30)]
        public string? ShebaNumber { get; set; }
        // FKs and relations
        // Owner
        public string? OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }
        // Subscription
        public Subscription? Subscription { get; set; }
        public int RestaurantCategoryId { get; set; }
        public RestaurantCategory RestaurantCategory { get; set; } = null!;
        // Foods
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        // Categories
        public ICollection<CustomFoodCategory> FoodCategories { get; set; } = new List<CustomFoodCategory>();
        // Ads
        public ICollection<RestaurantAd> Advertisements { get; set; } = new List<RestaurantAd>();
        // Ratings
        public ICollection<RestaurantRating> Ratings { get; set; } = new List<RestaurantRating>();
        [NotMapped]
        public double AverageRating => Ratings.Any() ? Ratings.Average(r => r.Score) : 0;
        [NotMapped]
        public int VotersCount => Ratings.Count;
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        // connection to Orders from specific restaurant
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        // Tables
        public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
    }
}
