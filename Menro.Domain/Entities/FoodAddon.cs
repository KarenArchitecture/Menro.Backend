using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;
    }
}
