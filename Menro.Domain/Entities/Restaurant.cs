using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Restaurant
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

        [Display(Name = "آدرس عکس")]
        public string? BannerImageUrl { get; set; }

        [Display(Name = "آدرس عکس بنر تبلیغاتی ")]
        public string? CarouselImageUrl { get; set; }

        [Required(ErrorMessage = "افزودن آدرس رستوران الزامی است")]
        public string Address { get; set; } = string.Empty;

        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public DateTime CreatedAt { get; set; }
        [Display(Name = "توضیحات")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;  // New: to mark if restaurant is active or logically deleted

        [Display(Name = "تأیید شده")]
        public bool IsApproved { get; set; } = false;  // New: to indicate admin approval status

        // مشخصات صاحب رستوران
        [Display(Name = "کد ملی")]
        [MaxLength(10)]
        [Required(ErrorMessage = "کد ملی الزامی است")]
        public string NationalCode { get; set; } = string.Empty;

        [Display(Name = "کد ملی")]
        [MaxLength(15)]
        [Required(ErrorMessage = "شماره حساب الزامی است")]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Display(Name = "شماره شبا")]
        [MaxLength(30)]
        public string? ShebaNumber { get; set; }

        // اتصال رستوران به صاحب رستوران
        public string OwnerUserId { get; set; } = string.Empty;

        public User OwnerUser { get; set; } = null!;

        // اشتراک
        public Subscription? Subscription { get; set; }

        public bool IsFeatured { get; set; } = false;

        // دسته بندی رستوران (Navigation property + FK)
        public int RestaurantCategoryId { get; set; }

        public RestaurantCategory RestaurantCategory { get; set; } = null!;

        // غذاها
        public ICollection<Food> Foods { get; set; } = new List<Food>();

        public ICollection<FoodCategory> FoodCategories { get; set; } = new List<FoodCategory>();

        public ICollection<RestaurantRating> Ratings { get; set; } = new List<RestaurantRating>();

        public ICollection<RestaurantDiscount> Discounts { get; set; } = new List<RestaurantDiscount>();

        public RestaurantAdBanner? AdBanner { get; set; }

    }
}
