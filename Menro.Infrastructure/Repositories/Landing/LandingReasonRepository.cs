using Menro.Domain.Interfaces.Landing;
using Menro.Domain.Entities.Landing;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.Landing
{
    public class LandingReasonRepository : ILandingReasonRepository
    {
        private readonly MenroDbContext _context;

        public LandingReasonRepository(MenroDbContext context)
        {
            _context = context;
        }

        public Task<List<LandingReason>> GetAllOrderedAsync() =>
            _context.LandingReasons
                .OrderBy(r => r.SortOrder)
                .ToListAsync();

        public Task<LandingReason?> GetByIdAsync(Guid id) =>
            _context.LandingReasons.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<int> GetNextSortOrderAsync()
        {
            var max = await _context.LandingReasons
                .Select(r => (int?)r.SortOrder)
                .MaxAsync();
            return (max ?? -1) + 1;
        }

        public async Task AddAsync(LandingReason entity)
        {
            await _context.LandingReasons.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LandingReason entity)
        {
            _context.LandingReasons.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(LandingReason first, LandingReason second)
        {
            _context.LandingReasons.Update(first);
            _context.LandingReasons.Update(second);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LandingReason entity)
        {
            _context.LandingReasons.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
