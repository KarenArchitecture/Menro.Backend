using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces
{
    public interface IMusicTrackRepository
    {
        Task<MusicTrack?> GetByIdAsync(Guid id, int restaurantId);

        Task<List<MusicTrack>> GetAllByRestaurantIdAsync(int restaurantId);

        Task AddAsync(MusicTrack track);

        void Update(MusicTrack track);

        bool Remove(MusicTrack track);

        Task SaveChangesAsync();
    }
}
