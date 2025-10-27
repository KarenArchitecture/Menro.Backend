using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class Icon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Label { get; set; }
    }
}
