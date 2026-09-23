using System;

namespace Menro.Domain.Entities
{
    public class RestaurantRating
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public int Score { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🆕 برای اینکه بدونیم کاربر رایش رو بعداً اصلاح کرده یا نه
        public DateTime? UpdatedAt { get; set; }
    }
}