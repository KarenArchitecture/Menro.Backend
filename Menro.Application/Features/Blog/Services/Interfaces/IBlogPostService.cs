using Menro.Application.DTOs.Blog;

namespace Menro.Application.Features.Blog.Services
{
    public interface IBlogPostService
    {
        Task<IReadOnlyList<BlogPostResponse>> GetAllAsync(
            string? search, Guid? categoryId, CancellationToken ct = default);

        Task<BlogPostResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<BlogPostResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default);

        Task<BlogPostResponse?> UpdateAsync(Guid id, UpdateBlogPostRequest request, CancellationToken ct = default);

        Task<BlogPostResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
