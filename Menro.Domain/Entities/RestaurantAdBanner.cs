using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class RestaurantAdBanner
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!; // required nav

        public string? ImageUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [MaxLength(200)]
        public string? CommercialText { get; set; }
        public int PurchasedViews { get; set; } = 0;  // 0 => unlimited
        public int ConsumedViews { get; set; } = 0;   // incremented on impression

        public bool IsPaused { get; set; } = false;   // for future control

        [NotMapped] // computed; don't query with this in EF LINQ
        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    }
}
