using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// A "display category" card shown under the hero, above the feed
    /// ("دسته‌بندی‌های نمایشی" tab). Purely presentational - not linked to posts.
    /// </summary>
    public class BlogCategory
    {
        public Guid Id { get; set; }

        // Kept in sync with CreateBlogCategoryRequest/UpdateBlogCategoryRequest -
        // these are fixed-size cards on the blog page, so length is capped at
        // the domain level too, not just at the request-validation boundary.
        // NOTE: if column lengths are also configured via EF Core Fluent API
        // (e.g. HasMaxLength in a BlogCategoryConfiguration class), update
        // those to match - that configuration wasn't available here.
        [MaxLength(30)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>Hex color string, e.g. "#5A302F".</summary>
        public string ColorHex { get; set; } = string.Empty;

        /// <summary>
        /// Controls display order (the up/down reorder buttons in the admin list
        /// map directly to this).
        /// </summary>
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
