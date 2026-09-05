namespace Menro.Domain.Entities.SiteContent
{
    /// <summary>
    /// Singleton settings row that drives the general/static texts of the public
    /// landing page (hero image + hero title + the "با منرو تو چشم باش" heading).
    /// There is always exactly one row in this table - see
    /// LandingGeneralRepository.GetOrCreateAsync().
    /// </summary>
    public class LandingGeneral
    {
        /// <summary>
        /// Fixed id used to seed (and always look up) the single settings row.
        /// </summary>
        public static readonly Guid SingletonId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        public Guid Id { get; set; } = SingletonId;

        /// <summary>
        /// Bare file name only (never a full URL) - resolved to a URL on the way
        /// out via IFileUrlService.BuildLandingHeroImageUrl, same convention as
        /// BlogPost.CoverImageFileName.
        /// </summary>
        public string? HeroImageFileName { get; set; }

        /// <summary>The highlighted (orange) word in the hero title, e.g. "منرو".</summary>
        public string HeroHighlight { get; set; } = string.Empty;

        /// <summary>The rest of the hero title, e.g. "بهترین همیار رستوران تو".</summary>
        public string HeroTitle { get; set; } = string.Empty;

        /// <summary>Heading of the "با منرو تو چشم باش" showcase section.</summary>
        public string SpotlightTitle { get; set; } = string.Empty;

        public DateTime UpdatedAtUtc { get; set; }
    }
}
