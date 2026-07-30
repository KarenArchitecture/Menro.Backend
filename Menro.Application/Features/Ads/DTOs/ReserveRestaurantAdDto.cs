
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Ads.DTOs
{
    public class ReserveRestaurantAdDto
    {
        public int RestaurantId { get; set; }
        public AdPlacementType PlacementType { get; set; }
        public AdBillingType BillingType { get; set; }
        public int Cost { get; set; }
        public IFormFile Image { get; set; } = null!;
        public int PurchasedUnits { get; set; }
        public string CommercialText { get; set; } = string.Empty;
    }

}

