namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantBannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }
        public int TableCount { get; set; }
    }
}


