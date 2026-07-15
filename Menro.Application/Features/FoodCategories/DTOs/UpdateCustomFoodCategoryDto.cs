namespace Menro.Application.Features.FoodCategories.DTOs
{
    public class UpdateCustomFoodCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? IconId { get; set; } = null;
    }

}
