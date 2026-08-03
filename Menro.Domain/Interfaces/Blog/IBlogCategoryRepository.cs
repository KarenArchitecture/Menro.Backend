using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    // NOTE: reconstructed from how BlogCategoryService.cs uses this interface -
    // your actual file may already have all the non-NEW members below in a
    // different order. Only the two methods marked NEW need to be added.
    public interface IBlogCategoryRepository
    {
        Task<IReadOnlyList<BlogCategory>> GetAllOrderedAsync(CancellationToken ct = default);
        Task<BlogCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<int> GetNextSortOrderAsync(CancellationToken ct = default);
        Task AddAsync(BlogCategory category, CancellationToken ct = default);
        Task UpdateAsync(BlogCategory category, CancellationToken ct = default);
        Task<int> CountByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
        Task DeleteAsync(BlogCategory category, CancellationToken ct = default);
        Task SwapSortOrderAsync(BlogCategory a, BlogCategory b, CancellationToken ct = default);

        // NEW - needed to resolve /blog/category/{slug} on the public site.
        Task<BlogCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);

        // NEW - used when generating a unique slug on create (see
        // BlogCategoryService.GenerateUniqueSlugAsync).
        Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    }
}
