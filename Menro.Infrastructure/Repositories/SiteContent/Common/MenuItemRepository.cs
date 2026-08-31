using Menro.Domain.Entities.SiteContent;
using Menro.Domain.Interfaces.SiteContent;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.SiteContent
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly MenroDbContext _context;

        public MenuItemRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItem>> GetByLocationAsync(MenuLocation location, bool includeInactive = false)
        {
            var query = _context.MenuItems
                .Where(x => x.Location == location);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetByIdAsync(Guid id)
        {
            return await _context.MenuItems
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems
                .OrderBy(x => x.Location)
                .ThenBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<int> GetMaxOrderAsync(MenuLocation location)
        {
            var hasAny = await _context.MenuItems
                .AnyAsync(x => x.Location == location);

            if (!hasAny)
                return 0;

            return await _context.MenuItems
                .Where(x => x.Location == location)
                .MaxAsync(x => x.Order);
        }

        public async Task AddAsync(MenuItem entity)
        {
            await _context.MenuItems.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MenuItem entity)
        {
            _context.MenuItems.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(MenuItem entity)
        {
            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<MenuItem> items)
        {
            _context.MenuItems.UpdateRange(items);
            await _context.SaveChangesAsync();
        }
    }
}