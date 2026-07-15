namespace Menro.Application.Features.Landing.DTOs
{
    public record LandingFaqResponse(
        Guid Id,
        string Question,
        string Answer,
        int SortOrder);

    public record CreateLandingFaqRequest(string Question, string Answer);

    public record UpdateLandingFaqRequest(string Question, string Answer);
}
