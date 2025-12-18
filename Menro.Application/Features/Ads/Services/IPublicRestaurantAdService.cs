using Menro.Application.Features.Ads.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Ads.Services
{
    public interface IPublicRestaurantAdService
    {
        Task<List<RestaurantAdCarouselDto>> GetCarouselAdsAsync(int take = 10);

        Task<RestaurantAdBannerDto?> GetRandomBannerAsync(IReadOnlyCollection<int> excludeAdIds);

        // Tracking:
        // - Impression consumes unit only when BillingType == PerView
        Task TrackBannerImpressionAsync(int adId);

        // - Click consumes unit only when BillingType == PerClick
        Task TrackBannerClickAsync(int adId);

        // Optional for future: carousel PerClick tracking
        Task TrackCarouselClickAsync(int adId);
    }
}
