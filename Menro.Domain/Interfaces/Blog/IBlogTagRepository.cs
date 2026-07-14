using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogTagRepository
    {
        Task<BlogTag?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<bool> ExistsByNameAsync(
            string name,
            Guid? excludingId = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetAllWithArticleCountsAsync(
            CancellationToken ct = default);

        Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetSuggestedWithArticleCountsAsync(
            CancellationToken ct = default);

        Task<int> CountSuggestedAsync(CancellationToken ct = default);

        Task AddAsync(
            BlogTag tag,
            CancellationToken ct = default);

        Task UpdateAsync(
            BlogTag tag,
            CancellationToken ct = default);

        Task DeleteAsync(
            BlogTag tag,
            CancellationToken ct = default);
    }
}