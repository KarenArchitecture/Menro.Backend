

using Menro.Application.Features.Icons.DTOs;

namespace Menro.Application.Features.GlobalFoodCategories.DTOs
{
    public class GlobalCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GetIconDto? Icon { get; set; }

    }
}
