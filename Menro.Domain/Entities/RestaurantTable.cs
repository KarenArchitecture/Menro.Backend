using Menro.Domain.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class RestaurantTable : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "برچسب میز")]
        [MaxLength(50)]
        [Required(ErrorMessage = "برچسب میز الزامی است")]
        public string Label { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        // FK
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
    }
}
