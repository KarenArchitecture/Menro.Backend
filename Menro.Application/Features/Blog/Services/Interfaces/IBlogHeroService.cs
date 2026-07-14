using Menro.Application.Features.Blog.DTOs;

namespace Menro.Application.Features.Blog.Services
{
    public interface IBlogHeroService
    {
        Task<BlogHeroResponse> GetAsync(
            CancellationToken ct = default);

        Task<BlogHeroResponse> UpdateAsync(
            UpdateBlogHeroRequest request,
            CancellationToken ct = default);
    }
}