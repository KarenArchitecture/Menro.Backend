using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Menro.Domain.Entities
{
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام آیتم")]
        [MaxLength(200)]
        [Required(ErrorMessage = "نام آیتم الزامی است")]
        public string Name { get; set; } = string.Empty;

        //[Display(Name = "توضیحات مختصر")]
        //[MaxLength(300)]
        //public string? Description { get; set; }

        [Display(Name = "ترکیبات")]
        [MaxLength(500)]
        public string? Ingredients { get; set; }

        [Display(Name = "قیمت ثابت")]
        public int Price { get; set; }   // فقط زمانی پر میشه که Variants نداشته باشه

        [Display(Name = "آدرس عکس")]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public bool HasVariant { get; set; }
        public bool IsAvailable { get; set; } = true;  // Shows if food can be ordered
        public bool IsDeleted { get; set; } = false;   // Soft delete flag

        // --- Computed (NotMapped) ---
        [NotMapped]
        public double AverageRating => Ratings.Any() ? Ratings.Average(r => r.Score) : 0;

        [NotMapped]
        public int VotersCount => Ratings.Count;

        // FK به رستوران
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        // FK: SpecialFoodCategory
        public int? CustomFoodCategoryId { get; set; }
        public CustomFoodCategory? CustomFoodCategory { get; set; } = null!;

        // FK: GlobalFoodCategory
        public int? GlobalFoodCategoryId { get; set; }
        public GlobalFoodCategory? GlobalFoodCategory { get; set; }

        // ارتباط‌ها
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<FoodRating> Ratings { get; set; } = new List<FoodRating>();

        // انواع (Variants) - مثلا سایز کوچک/بزرگ یا زعفرانی/دارچینی و غیره
        public ICollection<FoodVariant> Variants { get; set; } = new List<FoodVariant>();

    }
}