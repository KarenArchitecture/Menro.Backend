using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class SaveThemeDto
    {
        [Required]
        public string ThemeId { get; set; } = string.Empty;
    }
}