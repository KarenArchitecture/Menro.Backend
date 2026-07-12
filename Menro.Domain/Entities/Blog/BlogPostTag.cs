namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// Join entity linking a BlogPost to a BlogTag. Exists purely so BlogTag's
    /// article count can be computed (count of rows per BlogTagId) instead of
    /// stored as an editable field.
    /// </summary>
    public class BlogPostTag
    {
        public Guid Id { get; set; }

        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;

        public Guid BlogTagId { get; set; }
        public BlogTag BlogTag { get; set; } = null!;
    }
}
