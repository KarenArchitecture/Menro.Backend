using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.Music
{
    public class MusicPlayerRepository : IMusicPlayerRepository
    {
        private readonly MenroDbContext _context;
        public MusicPlayerRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(MusicPlayer player)
        {
            await _context.MusicPlayers.AddAsync(player);
            await _context.SaveChangesAsync();
        }

        public async Task<MusicPlayer?> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.MusicPlayers.FirstOrDefaultAsync(x => x.RestaurantId == restaurantId);
        }

        public Task UpdateAsync(MusicPlayer player)
        {
            _context.MusicPlayers.Update(player);

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
