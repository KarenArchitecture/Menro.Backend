using Menro.Application.Features.Ads.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/public/restaurant")]
    public class RestaurantPublicAdsController : ControllerBase
    {
        private readonly IPublicRestaurantAdService _service;

        public RestaurantPublicAdsController(IPublicRestaurantAdService service)
        {
            _service = service;
        }

        // Carousel data for HomePage Carousel.jsx
        // GET: /api/restaurant/featured
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int take = 10)
        {
            var list = await _service.GetCarouselAdsAsync(take);
            return Ok(list);
        }

        // Random banner for AdBanner.jsx
        // GET: /api/restaurant/ad-banner/random?exclude=1,2,3
        [HttpGet("ad-banner/random")]
        public async Task<IActionResult> GetRandomBanner([FromQuery] string? exclude)
        {
            var excludeRestaurantIds = ParseExcludeIds(exclude);

            var ad = await _service.GetRandomBannerAsync(excludeRestaurantIds);
            if (ad == null) return NoContent();

            return Ok(ad);
        }


        // PerView billing should increment here (only if BillingType == PerView)
        // POST: /api/restaurant/ad-banner/{id}/impression
        [HttpPost("ad-banner/{id:int}/impression")]
        public async Task<IActionResult> Impression([FromRoute] int id)
        {
            await _service.TrackBannerImpressionAsync(id);
            return Ok();
        }

        // PerClick billing should increment here (only if BillingType == PerClick)
        // POST: /api/restaurant/ad-banner/{id}/click
        [HttpPost("ad-banner/{id:int}/click")]
        public async Task<IActionResult> Click([FromRoute] int id)
        {
            await _service.TrackBannerClickAsync(id);
            return Ok();
        }

        // Optional: if you later decide to track carousel clicks for PerClick carousel ads
        // POST: /api/restaurant/carousel/{adId}/click
        [HttpPost("carousel/{adId:int}/click")]
        public async Task<IActionResult> CarouselClick([FromRoute] int adId)
        {
            await _service.TrackCarouselClickAsync(adId);
            return Ok();
        }

        private static List<int> ParseExcludeIds(string? exclude)
        {
            if (string.IsNullOrWhiteSpace(exclude))
                return new List<int>();

            return exclude
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();
        }
    }
}
