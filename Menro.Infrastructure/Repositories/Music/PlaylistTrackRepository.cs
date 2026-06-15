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


        // re-order track in playlist
        public async Task<PlaylistTrack?> GetPreviousTrackAsync(Guid playlistId, int sortOrder)
        {
            return await _context.PlaylistTracks
                .Where(x =>
                    x.PlaylistId == playlistId &&
                    x.SortOrder < sortOrder)
                .OrderByDescending(x => x.SortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<PlaylistTrack?> GetNextTrackAsync(Guid playlistId, int sortOrder)
        {
            return await _context.PlaylistTracks
                .Where(x =>
                    x.PlaylistId == playlistId &&
                    x.SortOrder > sortOrder)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefaultAsync();
        }


        public void Remove(PlaylistTrack entity)
        {
            _context.PlaylistTracks.Remove(entity);
        }
    }
}
