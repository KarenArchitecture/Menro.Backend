using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistTrackRepository
    {

        Task AddAsync(PlaylistTrack entity);
        Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId);
        Task UpdateAsync(PlaylistTrack request);
        void Remove(PlaylistTrack entity);
        Task<bool> RemoveByIdAsync(Guid playlistTrackId);

        Task<int> GetLastSortOrderAsync(Guid playlistId);

        // re-order track in playlist
        Task<PlaylistTrack?> GetPreviousTrackAsync(Guid playlistId, int sortOrder);

        Task<PlaylistTrack?> GetNextTrackAsync(Guid playlistId, int sortOrder);


        Task<List<PlaylistTrack>> GetAfterSortOrderAsync(Guid playlistId, int sortOrder);
        Task<List<PlaylistTrack>> GetRequestedTracksAfterCurrentAsync(Guid playlistId, int currentSortOrder);

    }
}
