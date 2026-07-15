namespace Menro.Application.Features.Foods.DTOs
{
    public class FoodCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int Price { get; set; }
        public string? ImageUrl { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }

        public string RestaurantName { get; set; } = string.Empty;
        public string RestaurantCategory { get; set; } = string.Empty;
    }
}
