using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.Blog
{
    public class BlogPostLikeRepository : IBlogPostLikeRepository
    {
        private readonly MenroDbContext _context;
        public BlogPostLikeRepository(MenroDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid postId, string userId, CancellationToken ct = default)
            => await _context.Set<BlogPostLike>()
                .AnyAsync(l => l.BlogPostId == postId && l.UserId == userId, ct);

        public async Task AddAsync(BlogPostLike like, CancellationToken ct = default)
            => await _context.Set<BlogPostLike>().AddAsync(like, ct);

        public async Task<bool> RemoveAsync(Guid postId, string userId, CancellationToken ct = default)
        {
            var existing = await _context.Set<BlogPostLike>()
                .FirstOrDefaultAsync(l => l.BlogPostId == postId && l.UserId == userId, ct);
            if (existing is null) return false;
            _context.Set<BlogPostLike>().Remove(existing);
            return true;
        }
    }
}
