namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantOverviewDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? ImageUrl { get; set; } = null;
    }
}