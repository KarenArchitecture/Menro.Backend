using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    // NOTE: assumes a DbContext called `MenroDbContext` with a
    // `DbSet<BlogPost> BlogPosts` property, and an `IBlogPostRepository`
    // interface matching the public members below - rename/adjust to match
    // your actual context and interface.
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly MenroDbContext _context;

        public BlogPostRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<IReadOnlyList<BlogPost>> GetAllAsync(
            string? search,
            Guid? categoryId,
            Guid? tagId = null,
            CancellationToken ct = default)
        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // NEW: tag filtering, mirrors the pattern already used in
            // CountByTagIdAsync below.
            if (tagId.HasValue)
                query = query.Where(p => p.PostTags.Any(pt => pt.BlogTagId == tagId.Value));

            return await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync(ct);
        }

        /// <summary>Used by BlogTagService to compute per-tag article counts.</summary>
        public async Task<int> CountByTagIdAsync(Guid tagId, CancellationToken ct = default)
        {
            return await _context.BlogPosts
                .Where(p => p.PostTags.Any(pt => pt.BlogTagId == tagId))
                .CountAsync(ct);
        }

        public async Task AddAsync(BlogPost post, CancellationToken ct = default)
        {
            await _context.BlogPosts.AddAsync(post, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(BlogPost post, CancellationToken ct = default)
        {
            post.UpdatedAtUtc = DateTime.UtcNow;
            _context.BlogPosts.Update(post);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(BlogPost post, CancellationToken ct = default)
        {
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync(ct);
        }

    }
}
