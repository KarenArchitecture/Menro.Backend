using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

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

        // -----------------------------
        // Carousel (row-based)
        // -----------------------------
        public async Task<List<RestaurantAdCarouselDto>> GetCarouselAdsAsync(int take = 10)
        {
            if (take <= 0) return new List<RestaurantAdCarouselDto>(0);

            var now = DateTime.UtcNow;

            // Row-based: every approved RestaurantAd row is eligible (no grouping by restaurant)
            var all = await _adsRepository.GetActiveApprovedAdsAsync(AdPlacementType.MainSlider, now);
            if (all.Count == 0) return new List<RestaurantAdCarouselDto>(0);

            // Pick a random subset without replacement (partial Fisher-Yates)
            var chosen = TakeRandom(all, take);

            var result = new List<RestaurantAdCarouselDto>(chosen.Count);
            foreach (var a in chosen)
            {
                result.Add(new RestaurantAdCarouselDto
                {
                    AdId = a.Id,
                    RestaurantId = a.RestaurantId,
                    RestaurantName = a.Restaurant.Name,
                    Slug = a.Restaurant.Slug,
                    ImageUrl = _fileUrlService.BuildAdImageUrl(a.ImageFileName),
                    TargetUrl = NormalizeTargetUrl(a.TargetUrl)
                });
            }

            return result;
        }

        // -----------------------------
        // Banner (row-based random)
        // exclude = AdIds (global dedupe on client)
        // -----------------------------
        public async Task<RestaurantAdBannerDto?> GetRandomBannerAsync(IReadOnlyCollection<int>? excludeAdIds)
        {
            var now = DateTime.UtcNow;
            excludeAdIds ??= Array.Empty<int>();

            var ad = await _adsRepository.GetRandomActiveApprovedAdAsync(
                AdPlacementType.FullscreenBanner,
                now,
                excludeAdIds);

            if (ad == null) return null;

            return new RestaurantAdBannerDto
            {
                AdId = ad.Id,
                RestaurantId = ad.RestaurantId,
                RestaurantName = ad.Restaurant.Name,
                Slug = ad.Restaurant.Slug,
                ImageUrl = _fileUrlService.BuildAdImageUrl(ad.ImageFileName),
                CommercialText = ad.CommercialText,
                TargetUrl = NormalizeTargetUrl(ad.TargetUrl)
            };
        }

        // -----------------------------
        // Tracking (AdId only)
        // -----------------------------
        public async Task TrackBannerImpressionAsync(int adId)
        {
            var now = DateTime.UtcNow;

            // Only consumes if that row is PerView; otherwise it's a no-op (expected).
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerView, now);
        }

        public async Task TrackBannerClickAsync(int adId)
        {
            var now = DateTime.UtcNow;

            // Only consumes if that row is PerClick; otherwise it's a no-op (expected).
            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerClick, now);
        }

        public async Task TrackCarouselClickAsync(int adId)
        {
            var now = DateTime.UtcNow;

            await _adsRepository.TryConsumeUnitsAsync(adId, 1, AdBillingType.PerClick, now);
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private static string? NormalizeTargetUrl(string? targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl)) return null;
            return targetUrl.Trim();
        }

        /// <summary>
        /// Random subset without replacement. Works fast even when all.Count is large.
        /// </summary>
        private static List<T> TakeRandom<T>(List<T> source, int take)
        {
            if (take >= source.Count) return source;

            // In-place partial shuffle on a copy to avoid mutating cached lists
            var arr = source.ToArray();
            var n = arr.Length;
            for (int i = 0; i < take; i++)
            {
                int j = Random.Shared.Next(i, n);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            var result = new List<T>(take);
            for (int i = 0; i < take; i++)
                result.Add(arr[i]);

            return result;
        }
    }
}
