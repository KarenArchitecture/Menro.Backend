using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class FoodAddon
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // مثلا "پنیر اضافه"

        [Required]
        public int ExtraPrice { get; set; }

        // FK
        public int FoodVariantId { get; set; }
        public FoodVariant FoodVariant { get; set; } = null!;
    }
}
