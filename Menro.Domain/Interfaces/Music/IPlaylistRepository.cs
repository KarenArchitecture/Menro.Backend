using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistRepository
    {
        Task<List<Playlist>> GetAllByRestaurantIdAsync(int restaurantId);
        Task<Playlist> GetByIdAsync(Guid playlistId);

    }
}
