using System.ComponentModel.DataAnnotations;

namespace Menro.Application.DTOs.Blog
{
    public record BlogCategoryResponse(
        Guid Id,
        string Title,
        string Subtitle,
        string ColorHex,
        int SortOrder);

    public class CreateBlogCategoryRequest
    {
        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "زیرعنوان الزامی است.")]
        [MaxLength(300)]
        public string Subtitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        public string ColorHex { get; set; } = "#5A302F";
    }

    public class UpdateBlogCategoryRequest
    {
        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "زیرعنوان الزامی است.")]
        [MaxLength(300)]
        public string Subtitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        public string ColorHex { get; set; } = string.Empty;
    }

    public enum MoveDirection
    {
        Up = -1,
        Down = 1
    }

    public class MoveBlogCategoryRequest
    {
        [Required]
        public MoveDirection Direction { get; set; }
    }
}
