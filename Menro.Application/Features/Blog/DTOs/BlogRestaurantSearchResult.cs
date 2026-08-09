namespace Menro.Application.Features.Blog.DTOs
{
    public record BlogRestaurantSearchResult(
        int Id,
        string Name,
        string CategoryName,
        string? LogoImageUrl,
        string? BannerImageUrl,
        double AverageRating,
        int VotersCount,
        string Slug);
}
