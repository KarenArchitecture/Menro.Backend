using System.ComponentModel.DataAnnotations;

namespace Menro.Application.DTOs.Blog
{
    public record BlogHeroResponse(
        Guid Id,
        string TitleLine,
        string Highlight,
        string SearchPlaceholder);

    public class UpdateBlogHeroRequest
    {
        [Required(ErrorMessage = "متن اصلی و متن هایلایت نباید خالی باشند.")]
        [MaxLength(200)]
        public string TitleLine { get; set; } = string.Empty;

        [Required(ErrorMessage = "متن اصلی و متن هایلایت نباید خالی باشند.")]
        [MaxLength(100)]
        public string Highlight { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SearchPlaceholder { get; set; } = string.Empty;
    }
}
