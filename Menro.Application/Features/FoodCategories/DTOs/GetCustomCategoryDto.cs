using Menro.Application.Features.Icons.DTOs;

namespace Menro.Application.Features.FoodCategories.DTOs
{
    public class GetCustomCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? GlobalCategoryId { get; set; }
        public GetIconDto? Icon { get; set; }
    }
}
