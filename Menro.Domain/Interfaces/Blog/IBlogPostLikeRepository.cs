using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogPostLikeRepository
    {
        Task<bool> ExistsAsync(Guid postId, string userId, CancellationToken ct = default);
        Task AddAsync(BlogPostLike like, CancellationToken ct = default);
        Task<bool> RemoveAsync(Guid postId, string userId, CancellationToken ct = default);
    }
}
