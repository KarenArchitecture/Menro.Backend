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

        public async Task<List<Playlist>> GetAllByRestaurantIdAsync(int restaurantId)
        {
            var entities = await _context.Playlists
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
        .FirstOrDefaultAsync(x => x.Id == playlistId);

            return playlist;
        }
    }
}
