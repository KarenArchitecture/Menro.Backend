using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogPostRepository
    {
        Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default);

        // CHANGED: added `tagId` parameter (default null, so existing callers
        // that don't care about tag-filtering keep compiling unchanged).
        Task<IReadOnlyList<BlogPost>> GetAllAsync(
            string? search,
            Guid? categoryId,
            Guid? tagId = null,
            CancellationToken ct = default);

        Task<int> CountByTagIdAsync(Guid tagId, CancellationToken ct = default);
        Task AddAsync(BlogPost post, CancellationToken ct = default);
        Task UpdateAsync(BlogPost post, CancellationToken ct = default);
        Task DeleteAsync(BlogPost post, CancellationToken ct = default);
    }
}
