using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly MenroDbContext _context;
        public BlogPostRepository(MenroDbContext context) => _context = context;

        public async Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.BlogTag)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<BlogPost?> GetByIdWithContentAsync(Guid id, CancellationToken ct = default)
            => await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Content)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<BlogPost>> GetPublishedWithTagsAsync(CancellationToken ct = default)
            => await _context.BlogPosts
                .Where(p => p.IsPublished)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.BlogTag)
                .ToListAsync(ct);

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludePostId = null, CancellationToken ct = default)
        {
            var query = _context.BlogPosts.Where(p => p.Slug == slug);
            if (excludePostId.HasValue)
                query = query.Where(p => p.Id != excludePostId.Value);
            return await query.AnyAsync(ct);
        }
        public async Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken ct = default)
            => await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Content)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.BlogTag)
                .FirstOrDefaultAsync(p => p.Slug == slug, ct);
        public async Task<IReadOnlyList<BlogPost>> GetAllAsync(
            string? search, Guid? categoryId, Guid? tagId = null, CancellationToken ct = default)
        {
            var query = _context.BlogPosts.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Title.Contains(search));
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (tagId.HasValue) query = query.Where(p => p.PostTags.Any(pt => pt.BlogTagId == tagId.Value));
            return await query.OrderByDescending(p => p.CreatedAtUtc).ToListAsync(ct);
        }

        public async Task<int> CountByTagIdAsync(Guid tagId, CancellationToken ct = default)
            => await _context.BlogPosts.Where(p => p.PostTags.Any(pt => pt.BlogTagId == tagId)).CountAsync(ct);

        /// <summary>برای هشدار حذف دسته‌بندی.</summary>
        public async Task<int> CountByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
            => await _context.BlogPosts.Where(p => p.CategoryId == categoryId).CountAsync(ct);

        public async Task AddAsync(BlogPost post, CancellationToken ct = default)
            => await _context.BlogPosts.AddAsync(post, ct);

        public Task AddPostTagAsync(BlogPostTag postTag, CancellationToken ct = default)
        {
            _context.BlogPostTags.Add(postTag);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(BlogPost post, CancellationToken ct = default)
        {
            post.UpdatedAtUtc = DateTime.UtcNow;
            _context.BlogPosts.Update(post);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(BlogPost post, CancellationToken ct = default)
        {
            _context.BlogPosts.Remove(post);
            return Task.CompletedTask;
        }
    }
}