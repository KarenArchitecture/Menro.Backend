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

        // Carousel data (row-based)
        // GET: /api/public/restaurant/featured?take=10
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int take = 10)
        {
            if (take <= 0) return Ok(Array.Empty<object>());

            var list = await _service.GetCarouselAdsAsync(take);
            return Ok(list);
        }

        // Random banner (exclude = AdIds)
        // GET: /api/public/restaurant/ad-banner/random?exclude=1,2,3
        [HttpGet("ad-banner/random")]
        public async Task<IActionResult> GetRandomBanner([FromQuery] string? exclude)
        
        {
            var excludeAdIds = ParseExcludeIds(exclude);

            var ad = await _service.GetRandomBannerAsync(excludeAdIds);
            if (ad == null) return NoContent();

            return Ok(ad);
        }

        // Impression (AdId only)
        // POST: /api/public/restaurant/ad-banner/{adId}/impression
        [HttpPost("ad-banner/{adId:int}/impression")]
        public async Task<IActionResult> Impression([FromRoute] int adId)
        {
            if (adId <= 0) return BadRequest();
            await _service.TrackBannerImpressionAsync(adId);
            return Ok();
        }

        // Click (AdId only)
        // POST: /api/public/restaurant/ad-banner/{adId}/click
        [HttpPost("ad-banner/{adId:int}/click")]
        public async Task<IActionResult> Click([FromRoute] int adId)
        {
            if (adId <= 0) return BadRequest();
            await _service.TrackBannerClickAsync(adId);
            return Ok();
        }

        // Carousel click (AdId only)
        // POST: /api/public/restaurant/carousel/{adId}/click
        [HttpPost("carousel/{adId:int}/click")]
        public async Task<IActionResult> CarouselClick([FromRoute] int adId)
        {
            if (adId <= 0) return BadRequest();
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
                .Where(v => v.HasValue && v.Value > 0)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();
        }
    }
}
