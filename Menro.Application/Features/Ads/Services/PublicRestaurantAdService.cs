// PublicRestaurantAdService.cs
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Menro.Application.Features.Ads.Services
{
    public class PublicRestaurantAdService : IPublicRestaurantAdService
    {
        private readonly IRestaurantAdRepository _adsRepository;
        private readonly IFileUrlService _fileUrlService;

        public PublicRestaurantAdService(IRestaurantAdRepository adsRepository, IFileUrlService fileUrlService)
        {
            _adsRepository = adsRepository;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<RestaurantAdCarouselDto>> GetCarouselAdsAsync(int take = 10)
        {
            var now = DateTime.UtcNow;

            // Carousel display is time-based => PerDay row
            var dayAds = await _adsRepository.GetActiveApprovedAdsAsync(
                AdPlacementType.MainSlider,
                AdBillingType.PerDay,
                now);

            // prevent duplicate restaurants (keep latest)
            var chosen = dayAds
                .GroupBy(a => a.RestaurantId)
                .Select(g => g.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).First())
                .OrderByDescending(x => x.StartDate)
                .Take(take)
                .ToList();

            var result = new List<RestaurantAdCarouselDto>();

            foreach (var a in chosen)
            {
                // enforce "time + click": must have paired PerClick
                var clickAdId = await _adsRepository.FindPairedAdIdAsync(a.Id, AdBillingType.PerClick, now);
                if (clickAdId == null) continue;

                result.Add(new RestaurantAdCarouselDto
                {
                    Id = a.Restaurant.Id,
                    Name = a.Restaurant.Name,
                    Slug = a.Restaurant.Slug,
                    CarouselImageUrl = _fileUrlService.BuildAdImageUrl(a.ImageFileName),
                    AdId = clickAdId.Value // frontend posts click using this id
                });
            }

            return result;
        }

        public async Task<RestaurantAdBannerDto?> GetRandomBannerAsync(IReadOnlyCollection<int> excludeRestaurantIds)
        {
            var now = DateTime.UtcNow;

            // Banner display is view-based => PerView row
            // Safety guard: avoid infinite loop when many PerView ads have no PerClick pair
            for (var i = 0; i < 20; i++)
            {
                var viewAd = await _adsRepository.GetRandomActiveApprovedAdAsync(
                    AdPlacementType.FullscreenBanner,
                    AdBillingType.PerView,
                    now,
                    excludeRestaurantIds ?? Array.Empty<int>());

                if (viewAd == null) return null;

                // enforce "view + click": must have paired PerClick
                var clickAdId = await _adsRepository.FindPairedAdIdAsync(viewAd.Id, AdBillingType.PerClick, now);
                if (clickAdId == null)
                {
                    excludeRestaurantIds = (excludeRestaurantIds ?? Array.Empty<int>()).Append(viewAd.RestaurantId).ToArray();
                    continue;
                }

                return new RestaurantAdBannerDto
                {
                    Id = viewAd.Id,                 // PerView id
                    RestaurantId = viewAd.RestaurantId,
                    ImageUrl = _fileUrlService.BuildAdImageUrl(viewAd.ImageFileName),
                    RestaurantName = viewAd.Restaurant.Name,
                    Slug = viewAd.Restaurant.Slug,
                    CommercialText = viewAd.CommercialText,
                    TargetUrl = string.IsNullOrWhiteSpace(viewAd.TargetUrl) ? null : viewAd.TargetUrl
                };
            }

            return null;
        }

        public async Task TrackBannerImpressionAsync(int adId)
        {
            var now = DateTime.UtcNow;
            // impression consumes PerView (adId is PerView id)
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerView, now);
        }

        public async Task TrackBannerClickAsync(int adId)
        {
            var now = DateTime.UtcNow;

            // adId is PerView id => find paired PerClick id, then consume click there
            var clickAdId = await _adsRepository.FindPairedAdIdAsync(adId, AdBillingType.PerClick, now);
            if (clickAdId == null) return;

            await _adsRepository.TryConsumeUnitsAsync(clickAdId.Value, 1, AdBillingType.PerClick, now);
        }

        public async Task TrackCarouselClickAsync(int adId)
        {
            var now = DateTime.UtcNow;
            // frontend sends PerClick id (AdId)
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerClick, now);
        }
    }
}
