using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Blog.DTOs
{
    public record BlogPostResponse(
        Guid Id,
        string Title,
        string? CoverImageUrl,
        int ReadingMinutes,
        Guid CategoryId,
        string CategoryTitle,
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        int ViewCount,
        int LikeCount);

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
        public Guid CategoryId { get; set; }

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
        public Guid CategoryId { get; set; }

        public bool IsPublished { get; set; }
    }
}
