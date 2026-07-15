using Menro.Application.Features.Foods.DTOs;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantMenuFoodQueryDto
    {
        public int FoodId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Ingredients { get; set; }

        public int BasePrice { get; set; }

        public string? ImageUrl { get; set; }

        public double Rating { get; set; }

        public int Voters { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryTitle { get; set; } = "نامشخص";

        public string? CategoryIconFileName { get; set; }

        public List<PublicFoodVariantDto> Variants { get; set; } = new();
    }
}
