using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Menro.Infrastructure.Repositories
{
    public class BlogCategoryRepository : IBlogCategoryRepository
    {
        private readonly MenroDbContext _context;

        public BlogCategoryRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<BlogCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.BlogCategories
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IReadOnlyList<BlogCategory>> GetAllOrderedAsync(CancellationToken ct = default)
        {
            return await _context.BlogCategories
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct);
        }

        public async Task<int> GetNextSortOrderAsync(CancellationToken ct = default)
        {
            var max = await _context.BlogCategories.MaxAsync(c => (int?)c.SortOrder, ct);
            return (max ?? 0) + 1;
        }

        public async Task AddAsync(BlogCategory category, CancellationToken ct = default)
        {
            await _context.BlogCategories.AddAsync(category, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(BlogCategory category, CancellationToken ct = default)
        {
            category.UpdatedAtUtc = DateTime.UtcNow;
            _context.BlogCategories.Update(category);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(BlogCategory category, CancellationToken ct = default)
        {
            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>Swaps SortOrder between two categories (used by the up/down move buttons).</summary>
        public async Task SwapSortOrderAsync(BlogCategory first, BlogCategory second, CancellationToken ct = default)
        {
            (first.SortOrder, second.SortOrder) = (second.SortOrder, first.SortOrder);
            first.UpdatedAtUtc = DateTime.UtcNow;
            second.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}
