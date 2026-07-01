using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;
using Menro.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Features;
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

        public async Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId)
        {
            var playlistTrack = await _context.PlaylistTracks
                .FirstOrDefaultAsync(x => x.Id == playlistTrackId);

            return playlistTrack;
        }

        public async Task<List<PlaylistTrack>> GetAllByMusicTrackId(Guid musicTrackId)
        {
            return await _context.PlaylistTracks
                .Where(x => x.MusicTrackId == musicTrackId)
                .ToListAsync();
        }


        public async Task<Guid> GetMusicTrackIdAsync(Guid playlistTrackId)
        {
            return await _context.PlaylistTracks
                .Where(x => x.Id == playlistTrackId)
                .Select(x => x.MusicTrackId)
                .FirstOrDefaultAsync();
        }

        public async Task<PlaylistTrack?> GetFirstByPlaylistIdAsync(Guid playlistId)
        {
            return await _context.PlaylistTracks
                .Where(t => t.PlaylistId == playlistId)
                .OrderBy(t => t.SortOrder)
                .FirstOrDefaultAsync();
        }
        public async Task<int> GetLastSortOrderAsync(Guid playlistId)
        {
            return await _context.PlaylistTracks
                .Where(x => x.PlaylistId == playlistId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync() ?? 0;
        }


        public Task UpdateAsync(PlaylistTrack request)
        {
            _context.PlaylistTracks.Update(request);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(PlaylistTrack playlistTrack)
        {
            _context.PlaylistTracks.Remove(playlistTrack);
            return Task.CompletedTask;
        }

        public Task RemoveRange(IEnumerable<PlaylistTrack> entity)
        {
            _context.PlaylistTracks.RemoveRange(entity);
            return Task.CompletedTask;
        }
        public async Task<bool> RemoveByIdAsync(Guid playlistTrackId)
        {
            try
            {
                var track = await _context.PlaylistTracks.FirstOrDefaultAsync(t => t.Id == playlistTrackId);
                if (track is null)
                {
                    return false;
                }
                _context.PlaylistTracks.Remove(track);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                throw;
            }
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


        public async Task<List<PlaylistTrack>> GetAfterSortOrderAsync(
            Guid playlistId,
            int sortOrder)
        {
            return await _context.PlaylistTracks
                .Where(x =>
                    x.PlaylistId == playlistId &&
                    x.SortOrder > sortOrder)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task<List<PlaylistTrack>> GetRequestedTracksAfterCurrentAsync(Guid playlistId, int currentSortOrder)
        {
            return await _context.PlaylistTracks
                .Where(x =>
                    x.PlaylistId == playlistId &&
                    x.IsRequestedTrack &&
                    x.SortOrder > currentSortOrder)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }
    }
}
