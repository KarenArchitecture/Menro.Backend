namespace Menro.Domain.Entities.Blog
{
    public class BlogPost
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public int ReadingMinutes { get; set; }

        public Guid CategoryId { get; set; }

        public BlogCategory? Category { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        // Navigation for BlogPostTag join rows - lets BlogTagService compute
        // per-tag article counts (see CountByTagIdAsync) and lets
        // BlogPostTagConfiguration's WithMany(x => x.PostTags) resolve.
        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
    }
}