using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistTrackRepository
    {

        Task AddAsync(PlaylistTrack entity);
        Task<int> GetLastSortOrderAsync(Guid playlistId);
        Task<PlaylistTrack?> GetByIdAsync(Guid playlistTrackId);
        void Remove(PlaylistTrack entity);
    }
}
