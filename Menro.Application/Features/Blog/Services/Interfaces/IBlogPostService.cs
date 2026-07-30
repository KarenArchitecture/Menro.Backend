using Menro.Application.Common;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Blog.DTOs;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Blog.Services
{
    // NOTE: reconstructed from BlogPostService.cs - reconcile with your actual
    // file. The only functional change is the new `tagId` parameter on
    // GetAllAsync (placed right after categoryId - update call sites
    // accordingly, see BlogPostsController and PublicBlogPostsController).
    public interface IBlogPostService
    {
        Task<PagedResult<BlogPostResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            Guid? tagId = null,
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
