using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogPostRepository
    {
        Task<BlogPost?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<IReadOnlyList<BlogPost>> GetAllAsync(
            string? searchTitle,
            BlogFeedCategory? category,
            CancellationToken ct = default);

        Task<int> CountByTagIdAsync(
            Guid tagId,
            CancellationToken ct = default);

        Task AddAsync(
            BlogPost post,
            CancellationToken ct = default);

        Task UpdateAsync(
            BlogPost post,
            CancellationToken ct = default);

        Task DeleteAsync(
            BlogPost post,
            CancellationToken ct = default);
    }
}