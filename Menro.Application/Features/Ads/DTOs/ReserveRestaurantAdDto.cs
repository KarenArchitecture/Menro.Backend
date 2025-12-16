
using Menro.Domain.Enums;

namespace Menro.Application.Features.Ads.DTOs
{
    public class ReserveRestaurantAdDto
    {
        public int RestaurantId { get; set; }
        public AdPlacementType PlacementType { get; set; }
        public AdBillingType BillingType { get; set; }
        public int Cost { get; set; }

        public string ImageFileName { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;

        public int PurchasedUnits { get; set; }
        public string CommercialText { get; set; } = string.Empty;
    }

}

