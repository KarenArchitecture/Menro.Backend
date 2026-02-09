namespace Menro.Application.Features.Ads.DTOs
{
    public class RestaurantAdBannerDto
    {
        // Identity (RestaurantAds row)
        public int AdId { get; set; }

        // UI
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = "";
        public string Slug { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string CommercialText { get; set; } = "";
        public string? TargetUrl { get; set; }
    }
}
