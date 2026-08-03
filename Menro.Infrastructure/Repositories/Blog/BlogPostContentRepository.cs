using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class BlogPostContentRepository : IBlogPostContentRepository
    {
        private readonly MenroDbContext _context;
        public BlogPostContentRepository(MenroDbContext context) => _context = context;

        public async Task<BlogPostContent?> GetByPostIdAsync(Guid postId, CancellationToken ct = default)
            => await _context.BlogPostContents.FirstOrDefaultAsync(c => c.BlogPostId == postId, ct);

        public async Task<BlogPostContent?> GetByPostIdWithPostAsync(Guid postId, CancellationToken ct = default)
            => await _context.BlogPostContents
                .Include(c => c.BlogPost).ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c => c.BlogPostId == postId, ct);

        public async Task AddAsync(BlogPostContent content, CancellationToken ct = default)
            => await _context.BlogPostContents.AddAsync(content, ct);

        public Task UpdateAsync(BlogPostContent content, CancellationToken ct = default)
        {
            _context.BlogPostContents.Update(content);
            return Task.CompletedTask;
        }
    }
}