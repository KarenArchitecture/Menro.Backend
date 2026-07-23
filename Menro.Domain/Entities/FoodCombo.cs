// Domain/Entities/FoodCombo.cs
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class FoodCombo
    {
        [Key]
        public int Id { get; set; }

        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public int ComboFoodId { get; set; }
        public Food ComboFood { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}