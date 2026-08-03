namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// Holds the raw HTML body of a BlogPost (from the Tiptap editor),
    /// split into its own table so list/feed queries (GetAllAsync) never
    /// have to load the heavy content column - those queries only touch
    /// BlogPost. Shared-PK 1-1 with BlogPost: BlogPostId IS the PK, so
    /// there's exactly one Content row per post, no orphans possible.
    /// </summary>
    public class BlogPostContent
    {
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;

        /// <summary>
        /// Raw HTML produced by the Tiptap editor, stored as-is.
        /// Sanitization/transformation (if any) happens at the
        /// service/application layer, not here.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}