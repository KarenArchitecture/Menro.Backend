using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantCategoryRepository : IRestaurantCategoryRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantCategoryRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<RestaurantCategory>> GetAllAsync()
        {
            return await _context.Set<RestaurantCategory>()
                .AsNoTracking()
                .ToListAsync();
        }

        // NOT no-tracking on purpose: the service mutates the returned
        // entity's Name and then calls SaveChangesAsync() without
        // re-attaching it, so EF needs to be tracking it.
        public async Task<RestaurantCategory?> GetByIdAsync(int id)
        {
            return await _context.Set<RestaurantCategory>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AnyAsync(Expression<Func<RestaurantCategory, bool>> predicate)
        {
            return await _context.Set<RestaurantCategory>().AnyAsync(predicate);
        }

        public async Task AddAsync(RestaurantCategory category)
        {
            await _context.Set<RestaurantCategory>().AddAsync(category);
        }

        public Task DeleteAsync(RestaurantCategory category)
        {
            _context.Set<RestaurantCategory>().Remove(category);
            return Task.CompletedTask;
        }

        // used to prevent duplicate category names (case-insensitive)
        // excludeId is used on edit, so the category being edited doesn't
        // collide with itself
        public async Task<bool> IsNameTakenAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();

            var query = _context.Set<RestaurantCategory>()
                .Where(c => c.Name.Trim().ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
