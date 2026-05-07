namespace Menro.Application.Features.Ads.DTOs
{
    public class RestaurantAdCarouselDto
    {
        // Identity (RestaurantAds row)
        public int AdId { get; set; }

        // UI
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = "";
        public string Slug { get; set; } = "";
        public string ImageUrl { get; set; } = "";

        // Optional (if some ads go somewhere other than /restaurant/{slug})
        public string? TargetUrl { get; set; }
    }
}
