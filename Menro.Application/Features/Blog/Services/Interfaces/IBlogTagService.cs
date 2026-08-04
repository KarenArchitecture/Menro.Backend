using Menro.Application.Features.Blog.DTOs;

namespace Menro.Application.Features.Blog.Services.Interfaces
{
    // NOTE: reconstructed from BlogTagService.cs - reconcile with your actual
    // file. Only GetBySlugAsync is NEW.
    public interface IBlogTagService
    {
        Task<IReadOnlyList<BlogTagResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<BlogTagResponse>> GetSuggestedAsync(CancellationToken ct = default);

        // NEW - resolves /blog/tag/{slug} on the public site.
        Task<BlogTagResponse?> GetBySlugAsync(string slug, CancellationToken ct = default);

        Task<BlogTagResponse> CreateAsync(CreateBlogTagRequest request, CancellationToken ct = default);
        Task<BlogTagResponse?> UpdateAsync(Guid id, UpdateBlogTagRequest request, CancellationToken ct = default);
        Task<BlogTagResponse?> ToggleSuggestedAsync(Guid id, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
