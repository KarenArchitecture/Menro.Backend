using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Blog.DTOs
{
    public class CreateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        // بقیه‌ی فیلدها حذف شدن - از مسیر Update ست میشن
    }

    public class UpdateBlogPostRequest
    {
        [Required(ErrorMessage = "عنوان پست الزامی است.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        public IFormFile? CoverImage { get; set; }
        public bool RemoveImage { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "زمان مطالعه باید بزرگ‌تر از صفر باشد.")]
        public int ReadingMinutes { get; set; }
        public Guid? CategoryId { get; set; }   // <- دیگه Required نیست، Nullable شد
        public bool IsPublished { get; set; }
    }

    // BlogPostResponse: چون CategoryId حالا Nullable شده، اینم باید هماهنگ بشه
    public record BlogPostResponse(
        Guid Id,
        string Title,
        string? CoverImageUrl,
        int ReadingMinutes,
        Guid? CategoryId,             // <- Nullable شد
        string? CategoryTitle,        // <- Nullable شد (وقتی دسته نداره، عنوانی هم نیست)
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        int ViewCount,
        int LikeCount,
        string PublishedDatePersian);
}
