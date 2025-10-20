using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class GlobalFoodCategory
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}