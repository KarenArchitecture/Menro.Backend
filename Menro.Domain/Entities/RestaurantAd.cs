using Menro.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class RestaurantAd
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public AdPlacementType PlacementType { get; set; }
        public AdBillingType BillingType { get; set; }
        public int Cost { get; set; } 

        public string ImageFileName { get; set; } = string.Empty;
        
        // rename target url to slug!
        public string TargetUrl { get; set; } = string.Empty;
        public string CommercialText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int PurchasedUnits { get; set; }
        public int ConsumedUnits { get; set; } 

        [MaxLength(400)]
        public string? AdminNotes{ get; set; }
        public AdStatus Status { get; set; } = AdStatus.Pending;

    }
}
