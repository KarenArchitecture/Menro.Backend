namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// A "suggested tag" shown in the sidebar and mobile random blocks
    /// ("برچسب‌های پیشنهادی" tab).
    /// </summary>
    public class BlogTag
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Join rows to BlogPost. Intentionally the ONLY source of the article
        /// count - "تعداد مقاله" is never stored on the tag itself, it's always
        /// derived by counting these at read time (see BlogTagRepository).
        /// </summary>
        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
    }
}
