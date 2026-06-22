using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.Music
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly MenroDbContext _context;

        public PlaylistRepository(MenroDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
        }

        public async Task<bool> ExistsAsync(int restaurantId, string name)
        {
            return await _context.Playlists.AnyAsync(x =>x.RestaurantId == restaurantId && x.Name == name);
        }
        public async Task<List<Playlist>> GetAllByRestaurantIdAsync(int restaurantId)
        {
            var entities = await _context.Playlists
                .Include(x => x.Tracks)
                .Where(x => x.RestaurantId == restaurantId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return entities;
        }

        public async Task<Playlist> GetByIdAsync(Guid playlistId)
        {
            var playlist = await _context.Playlists
        .Include(x => x.Tracks)
            .ThenInclude(x => x.MusicTrack)
        .FirstAsync(x => x.Id == playlistId);

            return playlist;
        }

        public Task<bool> UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);

            return Task.FromResult(true);
        }

        public Task DeleteAsync(Playlist playlist)
        {
            _context.Playlists.Remove(playlist);
            return Task.CompletedTask;
        }

        public async Task<Playlist?> GetActiveByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Playlists
                .FirstOrDefaultAsync(x =>
                    x.RestaurantId == restaurantId &&
                    x.IsActive);
        }
    }
}
