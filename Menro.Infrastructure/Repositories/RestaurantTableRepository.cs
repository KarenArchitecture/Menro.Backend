using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantTableRepository
        : Repository<RestaurantTable>, IRestaurantTableRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantTableRepository(MenroDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<List<RestaurantTable>> GetAllByRestaurantIdAsync(int restaurantId)
        {
            return await Set
                .Where(t => t.RestaurantId == restaurantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RestaurantTable?> GetByIdAsync(int id)
        {
            return await Set.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}