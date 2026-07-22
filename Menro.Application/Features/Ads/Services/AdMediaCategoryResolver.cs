using Menro.Application.Common.Media;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Ads.Services
{
    public static class AdMediaCategoryResolver
    {
        public static MediaCategory Resolve(AdPlacementType placementType) =>
            placementType switch
            {
                AdPlacementType.MainSlider => MediaCategory.RestaurantAdCarousel,
                AdPlacementType.FullscreenBanner => MediaCategory.RestaurantAdBanner,
                _ => MediaCategory.RestaurantAdBanner
            };
    }
}