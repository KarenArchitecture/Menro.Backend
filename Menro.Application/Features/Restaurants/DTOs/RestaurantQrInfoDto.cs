namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantQrInfoDto
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}
