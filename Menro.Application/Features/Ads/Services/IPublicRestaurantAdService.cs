using Menro.Application.Features.Ads.DTOs;

namespace Menro.Application.Features.Ads.Services
{
    public interface IPublicRestaurantAdService
    {
        // Row-based carousel ads (each RestaurantAd row is independent)
        // Keep "take" because your controller already supports it.
        Task<List<RestaurantAdCarouselDto>> GetCarouselAdsAsync(int take = 10);

        // Random banner (exclude = AdIds, not RestaurantIds)
        Task<RestaurantAdBannerDto?> GetRandomBannerAsync(IReadOnlyCollection<int>? excludeAdIds);

        // Tracking (AdId only)
        Task TrackBannerImpressionAsync(int adId);
        Task TrackBannerClickAsync(int adId);
        Task TrackCarouselClickAsync(int adId);
    }
}
