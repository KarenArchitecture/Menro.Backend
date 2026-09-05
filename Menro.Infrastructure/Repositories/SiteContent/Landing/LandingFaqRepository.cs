using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Menro.Domain.Entities.SiteContent;
using Menro.Domain.Interfaces.SiteContent;

namespace Menro.Infrastructure.Repositories.SiteContent
{
    public class LandingFaqRepository : ILandingFaqRepository
    {
        private readonly MenroDbContext _context;

        public LandingFaqRepository(MenroDbContext context)
        {
            _context = context;
        }

        public Task<List<LandingFaq>> GetAllOrderedAsync() =>
            _context.LandingFaqs
                .OrderBy(f => f.SortOrder)
                .ToListAsync();

        public Task<LandingFaq?> GetByIdAsync(Guid id) =>
            _context.LandingFaqs.FirstOrDefaultAsync(f => f.Id == id);

        public async Task<int> GetNextSortOrderAsync()
        {
            var max = await _context.LandingFaqs
                .Select(f => (int?)f.SortOrder)
                .MaxAsync();
            return (max ?? -1) + 1;
        }

        public async Task AddAsync(LandingFaq entity)
        {
            await _context.LandingFaqs.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LandingFaq entity)
        {
            _context.LandingFaqs.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(LandingFaq first, LandingFaq second)
        {
            _context.LandingFaqs.Update(first);
            _context.LandingFaqs.Update(second);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LandingFaq entity)
        {
            _context.LandingFaqs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
