using Menro.Application.Features.Blog.DTOs;

namespace Menro.Application.Features.Blog.Services.Interfaces
{
    public interface IBlogRestaurantSearchService
    {
        Task<IReadOnlyList<BlogRestaurantSearchResult>> SearchAsync(
            string? term, int take = 10, CancellationToken ct = default);

        Task<BlogRestaurantSearchResult?> GetByIdAsync(int id, CancellationToken ct = default);

    }
}
