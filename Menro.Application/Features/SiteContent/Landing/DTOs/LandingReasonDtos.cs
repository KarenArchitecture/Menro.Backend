namespace Menro.Application.Features.SiteContent.DTOs
{
    public record LandingReasonResponse(
        Guid Id,
        string Icon,
        string ColorHex,
        string Title,
        string Description,
        int SortOrder);

    public record CreateLandingReasonRequest(
        string Icon,
        string ColorHex,
        string Title,
        string Description);

    public record UpdateLandingReasonRequest(
        string Icon,
        string ColorHex,
        string Title,
        string Description);
}
