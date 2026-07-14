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

        /// <summary>
        /// URL-friendly, Latin-only identifier generated once from Name at
        /// creation time (see BlogTagService.CreateAsync). Deliberately NOT
        /// regenerated when Name changes later, so public links to
        /// /blog/tag/{slug} keep working. Unique - see BlogTagConfiguration.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Whether this tag is featured as a "suggested tag" on the public blog
        /// sidebar. Nullable for now (existing rows have no value yet); treated
        /// as false when null.
        /// </summary>
        public bool? Suggested { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
    }
}
