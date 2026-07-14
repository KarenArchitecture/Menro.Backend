using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    // NOTE: reconstructed from how BlogTagService.cs uses this interface -
    // your actual file may already have all the non-NEW members below in a
    // different order. Only the two methods marked NEW need to be added.
    public interface IBlogTagRepository
    {
        Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetAllWithArticleCountsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetSuggestedWithArticleCountsAsync(CancellationToken ct = default);
        Task<BlogTag?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default);
        Task<int> CountSuggestedAsync(CancellationToken ct = default);
        Task AddAsync(BlogTag tag, CancellationToken ct = default);
        Task UpdateAsync(BlogTag tag, CancellationToken ct = default);
        Task DeleteAsync(BlogTag tag, CancellationToken ct = default);

        // NEW - needed to resolve /blog/tag/{slug} on the public site.
        Task<BlogTag?> GetBySlugAsync(string slug, CancellationToken ct = default);

        // NEW - used when generating a unique slug on create (see
        // BlogTagService.GenerateUniqueSlugAsync).
        Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    }
}
