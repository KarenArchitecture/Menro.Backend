using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Blog.DTOs
{
    public class CreateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسلاگ الزامی است.")]
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;
        public IFormFile? CoverImage { get; set; }
        public bool RemoveImage { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "زمان مطالعه باید بزرگ‌تر از صفر باشد.")]
        public int ReadingMinutes { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new();
        public bool IsPublished { get; set; }
    }
    public record BlogPostAdminListItemResponse(
    Guid Id,
    string Title,
    string? ThumbnailUrl,
    int ReadingMinutes,
    Guid? CategoryId,
    string? CategoryTitle,
    string? AuthorId,
    string AuthorName,
    bool IsPublished,
    DateTime CreatedAtUtc,
    string PublishedDatePersian);

    public record BlogPostListItemResponse(
        Guid Id,
        string Title,
        string Slug,
        string? CoverImageUrl,
        int ReadingMinutes,
        Guid? CategoryId,
        string? CategoryTitle,
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        int ViewCount,
        int LikeCount,
        string PublishedDatePersian);

    public record BlogPostDetailResponse(
        Guid Id,
        string Title,
        string Slug,
        string? CoverImageUrl,
        string? AuthorId, 
        string? AuthorName,
        int ReadingMinutes,
        Guid? CategoryId,
        string? CategoryTitle,
        IReadOnlyList<BlogPostTagResponse> Tags,
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        int ViewCount,
        int LikeCount,
        string PublishedDatePersian);


    public record BlogPostTagResponse(Guid Id, string Name);

    public record BlogPostPublishResponse(Guid Id, bool IsPublished);
}
