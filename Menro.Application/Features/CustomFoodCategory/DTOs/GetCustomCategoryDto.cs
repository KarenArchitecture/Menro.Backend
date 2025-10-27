namespace Menro.Application.Features.CustomFoodCategory.DTOs
{
    public class GetCustomCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? IconId { get; set; } = 0;
    }
}
