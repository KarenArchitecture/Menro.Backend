using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogCategoryRepository
    {
        Task<BlogCategory?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<IReadOnlyList<BlogCategory>> GetAllOrderedAsync(
            CancellationToken ct = default);

        Task<int> GetNextSortOrderAsync(
            CancellationToken ct = default);

        Task AddAsync(
            BlogCategory category,
            CancellationToken ct = default);

        Task UpdateAsync(
            BlogCategory category,
            CancellationToken ct = default);

        Task DeleteAsync(
            BlogCategory category,
            CancellationToken ct = default);

        Task SwapSortOrderAsync(
            BlogCategory first,
            BlogCategory second,
            CancellationToken ct = default);
    }
}