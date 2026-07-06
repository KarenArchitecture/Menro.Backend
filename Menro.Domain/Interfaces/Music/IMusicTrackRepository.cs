using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IMusicTrackRepository
    {
        Task<MusicTrack?> GetByIdAsync(Guid id, int restaurantId);

        Task<List<MusicTrack>> GetAllByRestaurantIdAsync(int restaurantId);

        Task AddAsync(MusicTrack track);

        Task UpdateAsync(MusicTrack track);

        bool Remove(MusicTrack track);

        Task SaveChangesAsync();
    }
}
