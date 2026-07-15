namespace Menro.Application.Features.Landing.DTOs
{
    /// <summary>
    /// GET /api/admin/landing/general response.
    /// HeroImageUrl is a ready-to-use URL (built server-side via IFileUrlService) -
    /// same convention as BlogPostResponse.CoverImageUrl.
    /// </summary>
    public record LandingGeneralResponse(
        Guid Id,
        string? HeroImageUrl,
        string HeroHighlight,
        string HeroTitle,
        string SpotlightTitle);

    /// <summary>
    /// PUT /api/admin/landing/general request.
    /// HeroImageFileName must be the bare file name previously returned by
    /// POST /api/admin/landing/general/hero-image - never a full URL.
    /// Pass null/empty to remove the hero image entirely.
    /// </summary>
    public record UpdateLandingGeneralRequest(
        string HeroHighlight,
        string HeroTitle,
        string SpotlightTitle,
        string? HeroImageFileName);

    /// <summary>
    /// POST /api/admin/landing/general/hero-image response.
    /// </summary>
    public record UploadLandingHeroImageResponse(string FileName, string Url);
}
