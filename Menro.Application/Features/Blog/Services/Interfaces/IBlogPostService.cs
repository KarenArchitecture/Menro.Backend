using Menro.Application.Features.Blog.DTOs;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Blog.Services.Interfaces
{
    public interface IBlogPostService
    {
        Task<PagedResult<BlogPostListItemResponse>> GetAllAsync(
                string? search, Guid? categoryId, Guid? tagId = null,
                BlogPostSortOrder sort = BlogPostSortOrder.Newest,
                bool publishedOnly = false, int page = 1, int pageSize = 20,
                CancellationToken ct = default);
        Task<BlogPostDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<BlogPostDetailResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default);
        Task<BlogPostDetailResponse?> UpdateAsync(Guid id, UpdateBlogPostRequest request, CancellationToken ct = default);
        Task<BlogPostPublishResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        /* --- BLOG CONTENT --- */
        Task<BlogPostContentResponse?> GetContentAsync(Guid postId, CancellationToken ct = default);
        Task<BlogPostContentResponse?> UpdateContentAsync(
            Guid postId, UpdateBlogPostContentRequest request, CancellationToken ct = default);
        Task<BlogContentImageUploadResponse?> UploadContentImageAsync(
            Guid postId, IFormFile image, CancellationToken ct = default);
    }
}