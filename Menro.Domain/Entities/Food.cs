using Menro.Domain.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class Food : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Ingredients { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAvailable { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public int? CustomFoodCategoryId { get; set; }
        public CustomFoodCategory? CustomFoodCategory { get; set; }

        public int? GlobalFoodCategoryId { get; set; }
        public GlobalFoodCategory? GlobalFoodCategory { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<FoodRating> Ratings { get; set; } = new List<FoodRating>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public ICollection<FoodVariant> Variants { get; set; } = new List<FoodVariant>();

        [NotMapped]
        public double AverageRating => Ratings.Count != 0 ? Ratings.Average(r => r.Score) : 0;

        [NotMapped]
        public int VotersCount => Ratings.Count;
    }
}
