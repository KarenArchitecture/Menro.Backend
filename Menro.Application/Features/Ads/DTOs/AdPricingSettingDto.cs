using Menro.Domain.Enums;

namespace Menro.Application.Features.Ads.DTOs
{
    public class AdPricingSettingDto
    {
        public int? Id { get; set; }
        public AdPlacementType PlacementType { get; set; }
        public AdBillingType BillingType { get; set; }
        public int MinUnits { get; set; }
        public int MaxUnits { get; set; }
        public int UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
