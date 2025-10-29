using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.GlobalFoodCategories.DTOs
{
    public class CreateGlobalCategoryDTO
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public int? IconId { get; set; }
    }
}
