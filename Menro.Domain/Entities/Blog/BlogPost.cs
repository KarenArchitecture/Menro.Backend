using Menro.Domain.Enums;

namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// A single blog post ("پست‌های وبلاگ" tab).
    /// </summary>
    public class BlogPost
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public int ReadingMinutes { get; set; }

        /// <summary>
        /// Fixed feed filter this post belongs to (جدیدترین‌ها / محبوب‌ترین‌ها / ...).
        /// </summary>
        public BlogFeedCategory Category { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Join rows to BlogTag. Used only to derive each tag's article count -
        /// there's no tag-picker in the current post form, but the relation is kept
        /// so "تعداد مقاله" on BlogTag can be computed instead of stored.
        /// </summary>
        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
    }
}
