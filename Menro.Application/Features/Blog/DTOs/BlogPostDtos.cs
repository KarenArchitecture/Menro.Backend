using System.ComponentModel.DataAnnotations;
using Menro.Domain.Enums;

namespace Menro.Application.DTOs.Blog
{
    public record BlogPostResponse(
        Guid Id,
        string Title,
        string? CoverImageUrl,
        int ReadingMinutes,
        BlogFeedCategory Category,
        string CategoryLabel,
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);

    public class CreateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "زمان مطالعه باید بزرگ‌تر از صفر باشد.")]
        public int ReadingMinutes { get; set; }

        [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است.")]
        public BlogFeedCategory Category { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    public class UpdateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "زمان مطالعه باید بزرگ‌تر از صفر باشد.")]
        public int ReadingMinutes { get; set; }

        [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است.")]
        public BlogFeedCategory Category { get; set; }

        public bool IsPublished { get; set; }
    }
}
