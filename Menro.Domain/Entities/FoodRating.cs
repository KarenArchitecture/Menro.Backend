using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class FoodRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FoodId { get; set; }
        public Food Food { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
