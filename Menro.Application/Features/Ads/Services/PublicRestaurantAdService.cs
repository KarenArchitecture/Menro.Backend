using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Ads.Services
{
    public class PublicRestaurantAdService : IPublicRestaurantAdService
    {
        private readonly IRestaurantAdRepository _adsRepository;
        private readonly IFileUrlService _fileUrlService;

        public PublicRestaurantAdService(
            IRestaurantAdRepository adsRepository,
            IFileUrlService fileUrlService)
        {
            _adsRepository = adsRepository;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<RestaurantAdCarouselDto>> GetCarouselAdsAsync(int take = 10)
        {
            var now = DateTime.UtcNow;

            // Active + Approved + Placement(MainSlider) + RemainingUnits(PerClick/PerView)
            var ads = await _adsRepository.GetActiveApprovedAdsAsync(AdPlacementType.MainSlider, now);

            // Prevent duplicate restaurants in carousel: keep most recent ad per restaurant
            var chosen = ads
                .GroupBy(a => a.RestaurantId)
                .Select(g => g.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).First())
                .OrderByDescending(x => x.StartDate)
                .Take(take)
                .ToList();

            return chosen.Select(a => new RestaurantAdCarouselDto
            {
                Id = a.Restaurant.Id,
                Name = a.Restaurant.Name,
                Slug = a.Restaurant.Slug,
                CarouselImageUrl = _fileUrlService.BuildAdImageUrl(a.ImageFileName),
                AdId = a.Id
            }).ToList();
        }

        public async Task<RestaurantAdBannerDto?> GetRandomBannerAsync(IReadOnlyCollection<int> excludeAdIds)
        {
            var now = DateTime.UtcNow;

            var ad = await _adsRepository.GetRandomActiveApprovedAdAsync(
                AdPlacementType.FullscreenBanner,
                now,
                excludeAdIds ?? Array.Empty<int>());

            if (ad == null) return null;

            return new RestaurantAdBannerDto
            {
                Id = ad.Id,
                ImageUrl = _fileUrlService.BuildAdImageUrl(ad.ImageFileName),
                RestaurantName = ad.Restaurant.Name,
                Slug = ad.Restaurant.Slug,
                CommercialText = ad.CommercialText,
                TargetUrl = string.IsNullOrWhiteSpace(ad.TargetUrl) ? null : ad.TargetUrl
            };
        }

        public async Task TrackBannerImpressionAsync(int adId)
        {
            // Only PerView should consume on impression.
            // If the ad is not PerView, TryConsumeUnitsAsync returns false and we ignore it.
            var now = DateTime.UtcNow;
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerView, now);
        }

        public async Task TrackBannerClickAsync(int adId)
        {
            // Only PerClick should consume on click.
            var now = DateTime.UtcNow;
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerClick, now);
        }

        public async Task TrackCarouselClickAsync(int adId)
        {
            // Carousel click consumption (only if that ad row is PerClick)
            var now = DateTime.UtcNow;
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerClick, now);
        }
    }
}
