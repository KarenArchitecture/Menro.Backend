using Menro.Domain.Entities.SiteContent;
using Menro.Domain.Interfaces.SiteContent;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.SiteContent
{
    public class SiteLinkRepository : ISiteLinkRepository
    {
        private readonly MenroDbContext _context;

        public SiteLinkRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<SiteLink>> GetByLocationAsync(MenuLocation location, bool includeInactive = false)
        {
            var query = _context.SiteLinks
                .Where(x => x.Location == location);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<SiteLink?> GetByIdAsync(Guid id)
        {
            return await _context.SiteLinks
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<SiteLink>> GetAllAsync()
        {
            return await _context.SiteLinks
                .OrderBy(x => x.Location)
                .ThenBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<int> GetMaxOrderAsync(MenuLocation location)
        {
            var hasAny = await _context.SiteLinks
                .AnyAsync(x => x.Location == location);

            if (!hasAny)
                return 0;

            return await _context.SiteLinks
                .Where(x => x.Location == location)
                .MaxAsync(x => x.Order);
        }

        public async Task AddAsync(SiteLink entity)
        {
            await _context.SiteLinks.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SiteLink entity)
        {
            _context.SiteLinks.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(SiteLink entity)
        {
            _context.SiteLinks.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<SiteLink> items)
        {
            _context.SiteLinks.UpdateRange(items);
            await _context.SaveChangesAsync();
        }
    }
}