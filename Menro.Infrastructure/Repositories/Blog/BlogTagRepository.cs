using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class BlogTagRepository : IBlogTagRepository
    {
        private readonly MenroDbContext _context;

        public BlogTagRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<BlogTag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.BlogTags
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default)
        {
            var query = _context.BlogTags.Where(t => t.Name == name);
            if (excludingId.HasValue)
                query = query.Where(t => t.Id != excludingId.Value);
            return await query.AnyAsync(ct);
        }

        /// <summary>
        /// Returns every tag, alphabetically sorted by name, along with its live
        /// article count. "تعداد مقاله" is never read from a stored column -
        /// it's always this count.
        /// </summary>
        public async Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetAllWithArticleCountsAsync(
            CancellationToken ct = default)
        {
            var result = await _context.BlogTags
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    Tag = t,
                    ArticleCount = t.PostTags.Count
                })
                .ToListAsync(ct);

            return result.Select(x => (x.Tag, x.ArticleCount)).ToList();
        }

        public async Task<IReadOnlyList<(BlogTag Tag, int ArticleCount)>> GetSuggestedWithArticleCountsAsync(
            CancellationToken ct = default)
        {
            var result = await _context.BlogTags
                .Where(t => t.Suggested == true)
                .OrderBy(t => t.Name)
                .Select(t => new { Tag = t, ArticleCount = t.PostTags.Count })
                .ToListAsync(ct);

            return result.Select(x => (x.Tag, x.ArticleCount)).ToList();
        }

        /// <summary>
        /// Counts how many tags currently have Suggested == true. Used to enforce
        /// the sidebar's max-suggested-tags limit before flipping a tag on.
        /// </summary>
        public async Task<int> CountSuggestedAsync(CancellationToken ct = default)
        {
            return await _context.BlogTags.CountAsync(t => t.Suggested == true, ct);
        }

        public async Task AddAsync(BlogTag tag, CancellationToken ct = default)
        {
            await _context.BlogTags.AddAsync(tag, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(BlogTag tag, CancellationToken ct = default)
        {
            _context.BlogTags.Update(tag);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(BlogTag tag, CancellationToken ct = default)
        {
            _context.BlogTags.Remove(tag);
            await _context.SaveChangesAsync(ct);
        }
    }
}
