namespace Menro.Application.Features.Foods.DTOs
{
    public class HomeFoodCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string? RestaurantSlug { get; set; }
    }
}
