using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class GlobalFoodCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // to Icons
        public int? IconId { get; set; }

        [ForeignKey(nameof(IconId))]
        public Icon? Icon { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
