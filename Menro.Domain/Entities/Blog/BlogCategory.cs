namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// A "display category" card shown under the hero, above the feed
    /// ("دسته‌بندی‌های نمایشی" tab). Purely presentational - not linked to posts.
    /// </summary>
    public class BlogCategory
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

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
