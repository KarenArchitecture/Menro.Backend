using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class CustomFoodCategory
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // to Icons
        public int? IconId { get; set; }

        [ForeignKey(nameof(IconId))]
        public Icon? Icon { get; set; }

        // to Restaurants
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        // to Global Categories
        public int? GlobalCategoryId { get; set; }
        public GlobalFoodCategory? GlobalCategory { get; set; }

        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
