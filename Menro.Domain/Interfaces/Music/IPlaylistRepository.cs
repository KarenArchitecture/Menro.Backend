using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IPlaylistRepository
    {
        Task AddAsync(Playlist playlist);
        Task<bool> ExistsAsync(int restaurantId, string name);
        Task<List<Playlist>> GetAllByRestaurantIdAsync(int restaurantId);
        Task<Playlist> GetByIdAsync(Guid playlistId);
        Task<bool> UpdateAsync(Playlist playlist);
        Task DeleteAsync(Guid playlistId);



        Task<Playlist?> GetActiveByRestaurantIdAsync(int restaurantId);

    }
}
