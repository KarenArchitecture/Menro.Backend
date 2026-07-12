using Menro.Application.DTOs.Blog;

namespace Menro.Application.Features.Blog.Services
{
    public interface IBlogTagService
    {
        Task<IReadOnlyList<BlogTagResponse>> GetAllAsync(
            CancellationToken ct = default);

        Task<BlogTagResponse> CreateAsync(
            CreateBlogTagRequest request,
            CancellationToken ct = default);

        Task<BlogTagResponse?> UpdateAsync(
            Guid id,
            UpdateBlogTagRequest request,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);
    }
}