namespace Menro.Application.Features.Foods.DTOs
{
    public class RestaurantMenuItemDto
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Ingredients { get; init; }
        public int Price { get; init; }
        public string ImageUrl { get; init; }
    }
}
