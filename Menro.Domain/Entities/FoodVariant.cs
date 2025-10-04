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

        public bool IsAvailable { get; set; } = true;

        // مخلفات (Addons) - مثلا پنیر اضافه، نوشابه
        public ICollection<FoodAddon> Addons { get; set; } = new List<FoodAddon>();
        // FK
        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;
    }
}
