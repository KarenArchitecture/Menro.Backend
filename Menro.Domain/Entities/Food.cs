using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام آیتم")]
        [MaxLength(200)]
        [Required(ErrorMessage = "نام آیتم الزامی است")]
        public string Name { get; set; } = "کاربر منرو";

        [Display(Name = "ترکیبات")]
        [MaxLength(500)]
        public string? Ingredients { get; set; }

        [Display(Name = "قیمت")]
        [MaxLength(10)]
        [Required(ErrorMessage = "قیمت الزامی است")]
        public int Price { get; set; }

        [Display(Name = "آدرس عکس")]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAvailable { get; set; } = true;  // Shows if food can be ordered

        public bool IsDeleted { get; set; } = false;   // Soft delete flag

        // FK to Restaurant
        public int RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; } = null!;

        // FK to FoodCategory
        public int FoodCategoryId { get; set; }
        public FoodCategory FoodCategory { get; set; } = null!;

        // FK to Orders
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<FoodRating> Ratings { get; set; } = new List<FoodRating>();


    }
}
