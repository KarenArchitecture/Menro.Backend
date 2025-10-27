using Menro.Application.Features.Icons.DTOs;

namespace Menro.Application.Features.CustomFoodCategory.DTOs
{
    public class GetCustomCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? GlobalCategoryId { get; set; }
        public GetIconDto? Icon { get; set; }
    }
}
