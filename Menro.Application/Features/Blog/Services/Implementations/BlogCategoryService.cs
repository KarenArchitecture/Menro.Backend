using Menro.Application.Features.Blog.DTOs;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly IBlogCategoryRepository _repository;

        public BlogCategoryService(IBlogCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<BlogCategoryResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var categories = await _repository.GetAllOrderedAsync(ct);
            return categories.Select(ToResponse).ToList();
        }

        public async Task<BlogCategoryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var category = await _repository.GetByIdAsync(id, ct);
            return category is null ? null : ToResponse(category);
        }

        public async Task<BlogCategoryResponse> CreateAsync(
            CreateBlogCategoryRequest request, CancellationToken ct = default)
        {
            var category = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle.Trim(),
                ColorHex = request.ColorHex,
                SortOrder = await _repository.GetNextSortOrderAsync(ct),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repository.AddAsync(category, ct);
            return ToResponse(category);
        }

        public async Task<BlogCategoryResponse?> UpdateAsync(
            Guid id, UpdateBlogCategoryRequest request, CancellationToken ct = default)
        {
            var category = await _repository.GetByIdAsync(id, ct);
            if (category is null) return null;

            category.Title = request.Title.Trim();
            category.Subtitle = request.Subtitle.Trim();
            category.ColorHex = request.ColorHex;

            await _repository.UpdateAsync(category, ct);
            return ToResponse(category);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var category = await _repository.GetByIdAsync(id, ct);
            if (category is null) return false;

            await _repository.DeleteAsync(category, ct);
            return true;
        }

        /// <summary>
        /// Moves a category up or down by swapping SortOrder with its neighbor.
        /// Returns null if the category doesn't exist, or the (unchanged) current
        /// list if it's already at the top/bottom.
        /// </summary>
        public async Task<IReadOnlyList<BlogCategoryResponse>?> MoveAsync(
            Guid id, MoveDirection direction, CancellationToken ct = default)
        {
            var ordered = await _repository.GetAllOrderedAsync(ct);
            var index = ordered.ToList().FindIndex(c => c.Id == id);
            if (index == -1) return null;

            var list = ordered.ToList();
            var targetIndex = index + (int)direction;

            if (targetIndex < 0 || targetIndex >= list.Count)
                return list.Select(ToResponse).ToList(); // already at the edge, no-op

            await _repository.SwapSortOrderAsync(list[index], list[targetIndex], ct);

            var refreshed = await _repository.GetAllOrderedAsync(ct);
            return refreshed.Select(ToResponse).ToList();
        }

        private static BlogCategoryResponse ToResponse(BlogCategory category) => new(
            category.Id,
            category.Title,
            category.Subtitle,
            category.ColorHex,
            category.SortOrder);
    }
}
