using System.ComponentModel.DataAnnotations;


namespace Menro.Domain.Entities
{
    public class FoodVariant
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // مثلا "سایز کوچک"

        [Required]
        public int Price { get; set; }

        // FK
        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;
    }
}
