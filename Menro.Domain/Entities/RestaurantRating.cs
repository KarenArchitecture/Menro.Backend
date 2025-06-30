using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class RestaurantRating
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int Score { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
