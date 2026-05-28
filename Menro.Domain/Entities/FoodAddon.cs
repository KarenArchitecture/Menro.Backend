using System.ComponentModel.DataAnnotations;
using Menro.Domain.Interfaces.Persistence;

namespace Menro.Domain.Entities
{
    public class FoodAddon : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // مثلا "پنیر اضافه"

        [Required]
        public int ExtraPrice { get; set; }
        public bool IsDeleted { get; set; } = false;

        // FK
        public int FoodVariantId { get; set; }
        public FoodVariant FoodVariant { get; set; } = null!;
    }
}
