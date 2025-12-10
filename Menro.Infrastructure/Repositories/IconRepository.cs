using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class IconRepository : IIconRepository
    {
        private readonly MenroDbContext _context;

        public IconRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<Icon>> GetAllAsync()
        {
            return await _context.Icons.AsNoTracking().ToListAsync();
        }

        public async Task<Icon?> GetByIdAsync(int id)
        {
            return await _context.Icons.FindAsync(id);
        }

        public async Task<bool> AddAsync(Icon icon)
        {
            try
            {
                _context.Icons.Add(icon);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var icon = await _context.Icons.FindAsync(id);
            if (icon == null) return false;

            _context.Icons.Remove(icon);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
