using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.GlobalFoodCategories.DTOs
{
    public class CreateGlobalCategoryDTO
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
