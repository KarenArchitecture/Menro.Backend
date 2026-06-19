using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IMusicPlayerRepository
    {
        Task<MusicPlayer?> GetByRestaurantIdAsync(int restaurantId);

        Task CreateAsync(MusicPlayer player);

        Task UpdateAsync(MusicPlayer player);
        Task SaveChangesAsync();

    }
}
