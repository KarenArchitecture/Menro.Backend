namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// The hero title/highlight/search-placeholder text for the blog page
    /// ("هیرو و جستجو" tab). Designed as a singleton row - see BlogHeroRepository
    /// and BlogHeroService for how the single-row invariant is enforced.
    /// </summary>
    public class BlogHero
    {
        public Guid Id { get; set; }

        public string TitleLine { get; set; } = string.Empty;

        public string Highlight { get; set; } = string.Empty;

        public string SearchPlaceholder { get; set; } = string.Empty;

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
