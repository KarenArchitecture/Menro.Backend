using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories.Music
{
    public class PlaylistTrackRepository : IPlaylistTrackRepository
    { 
        private readonly MenroDbContext _context;

        public PlaylistTrackRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PlaylistTrack entity)
        {
            await _context.PlaylistTracks.AddAsync(entity);
        }

        public async Task<int> GetLastSortOrderAsync(Guid playlistId)
        {
            return await _context.PlaylistTracks
                .Where(x => x.PlaylistId == playlistId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync() ?? 0;
        }

        public async Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId)
        {
            var playlistTrack = await _context.PlaylistTracks
                .FirstOrDefaultAsync(x => x.Id == playlistTrackId);

            return playlistTrack;
        }

        public void Remove(PlaylistTrack entity)
        {
            _context.PlaylistTracks.Remove(entity);
        }
    }
}
