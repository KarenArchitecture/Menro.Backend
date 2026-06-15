using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistTrackRepository
    {

        Task AddAsync(PlaylistTrack entity);
        Task<int> GetLastSortOrderAsync(Guid playlistId);
        Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId);

        // re-order track in playlist
        Task<PlaylistTrack?> GetPreviousTrackAsync(Guid playlistId, int sortOrder);

        Task<PlaylistTrack?> GetNextTrackAsync(Guid playlistId, int sortOrder);
        void Remove(PlaylistTrack entity);
    }
}
