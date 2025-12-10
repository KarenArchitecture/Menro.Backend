namespace Menro.Application.Features.GlobalFoodCategories.DTOs
{
    public class UpdateGlobalCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? IconId { get; set; } = null;

    }
}
