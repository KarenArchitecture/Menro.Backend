namespace Menro.Application.Features.FoodCategories.DTOs
{
    public class RestaurantFoodCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public bool IsGlobal { get; set; } // true if it's a global category
    }
}
