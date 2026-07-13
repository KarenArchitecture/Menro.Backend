using Menro.Application.Features.Blog.DTOs;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Blog.Services
{
    public interface IBlogPostService
    {
        Task<PagedResult<BlogPostResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            bool publishedOnly = false,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<BlogPostResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<BlogPostResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default);
        Task<BlogPostResponse?> UpdateAsync(Guid id, UpdateBlogPostRequest request, CancellationToken ct = default);
        Task<BlogPostResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}