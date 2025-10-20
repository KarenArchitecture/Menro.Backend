namespace Menro.Application.Features.CustomFoodCategory.DTOs
{
    public class UpdateCustomFoodCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
    }

}
