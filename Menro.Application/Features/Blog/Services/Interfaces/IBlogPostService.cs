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
        
        Task<PagedResult<BlogPostAdminListItemResponse>> GetAllForAdminAsync(
            string? search, Guid? categoryId, Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            int page = 1, int pageSize = 20,
            string? currentUserId = null, bool isElevated = false,
            CancellationToken ct = default);
        
        Task<BlogPostDetailResponse?> GetByIdAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);

        Task<BlogPostDetailResponse> CreateAsync(CreateBlogPostRequest request, string authorId, CancellationToken ct = default);

        Task<BlogPostDetailResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request,
            string? currentUserId = null, bool isElevated = false, bool canPublish = false,
            CancellationToken ct = default);

        Task<BlogPostPublishResponse?> TogglePublishAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);

        Task<bool> DeleteAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);

        /* --- BLOG CONTENT --- */
        Task<BlogPostContentResponse?> GetContentAsync(
            Guid postId, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);

        Task<BlogPostContentResponse?> UpdateContentAsync(
            Guid postId, UpdateBlogPostContentRequest request,
            string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);

        Task<BlogContentImageUploadResponse?> UploadContentImageAsync(
            Guid postId, IFormFile image,
            string? currentUserId = null, bool isElevated = false, CancellationToken ct = default);
    }
}