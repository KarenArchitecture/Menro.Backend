using Menro.Application.Features.Blog.DTOs;

namespace Menro.Application.Features.Blog.Services
{
    public interface IBlogCategoryService
    {
        Task<IReadOnlyList<BlogCategoryResponse>> GetAllAsync(
            CancellationToken ct = default);

        Task<BlogCategoryResponse?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<BlogCategoryResponse> CreateAsync(
            CreateBlogCategoryRequest request,
            CancellationToken ct = default);

        Task<BlogCategoryResponse?> UpdateAsync(
            Guid id,
            UpdateBlogCategoryRequest request,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);

        Task<IReadOnlyList<BlogCategoryResponse>?> MoveAsync(
            Guid id,
            MoveDirection direction,
            CancellationToken ct = default);
    }
}