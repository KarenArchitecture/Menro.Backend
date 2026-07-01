using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistTrackRepository
    {

        Task AddAsync(PlaylistTrack entity);
        Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId);
        Task<List<PlaylistTrack>> GetAllByMusicTrackId(Guid musicTrackId);
        Task<Guid> GetMusicTrackIdAsync(Guid playlistTrackId);
        Task<PlaylistTrack?> GetFirstByPlaylistIdAsync(Guid playlistId);
        Task<int> GetLastSortOrderAsync(Guid playlistId);

        Task UpdateAsync(PlaylistTrack request);
        Task RemoveAsync(PlaylistTrack playlistTrack);
        Task RemoveRange(IEnumerable<PlaylistTrack> entity);
        Task<bool> RemoveByIdAsync(Guid playlistTrackId);


        // re-order track in playlist
        Task<PlaylistTrack?> GetPreviousTrackAsync(Guid playlistId, int sortOrder);

        Task<PlaylistTrack?> GetNextTrackAsync(Guid playlistId, int sortOrder);


        Task<List<PlaylistTrack>> GetAfterSortOrderAsync(Guid playlistId, int sortOrder);
        Task<List<PlaylistTrack>> GetRequestedTracksAfterCurrentAsync(Guid playlistId, int currentSortOrder);

    }
}
