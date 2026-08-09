namespace Menro.Domain.Entities.Blog
{
    public class BlogPost
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? AuthorId { get; set; } = string.Empty;
        public User? Author { get; set; }
        public string AuthorNameSnapshot { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public int ReadingMinutes { get; set; }
        public Guid? CategoryId { get; set; }
        public BlogCategory? Category { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
        public BlogPostContent? Content { get; set; }
    }
}