namespace Menro.Application.Features.FoodCategories.DTOs
{
    public class CreateCustomFoodCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int? IconId { get; set; }
        public int? GlobalCategoryId { get; set; }
    }
}
