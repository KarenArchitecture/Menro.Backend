namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public int? Discount { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public string? LogoImageUrl { get; set; }
        public bool IsOpen { get; set; }
        public string Slug { get; set; } = string.Empty;
    }
}
