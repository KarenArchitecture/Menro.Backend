using Menro.Domain.Enums;

namespace Menro.Domain.Rules
{
    public static class AdPricingRules
    {
        public static bool IsAllowed(AdPlacementType placement, AdBillingType billing)
        {
            return placement switch
            {
                AdPlacementType.MainSlider =>
                    billing == AdBillingType.PerDay || billing == AdBillingType.PerClick,

                AdPlacementType.FullscreenBanner =>
                    billing == AdBillingType.PerClick || billing == AdBillingType.PerView,

                _ => false
            };
        }
    }
}
