
namespace Menro.Application.Features.Ads.DTOs
{
    public class RestaurantAdListItemDto
    {
        public int Id { get; set; }
        public AdPlacementType PlacementType { get; set; }
        public AdBillingType BillingType { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int PurchasedUnits { get; set; }
        public int ConsumedUnits { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }

}
