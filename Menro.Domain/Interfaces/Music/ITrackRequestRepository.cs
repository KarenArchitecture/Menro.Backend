using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface ITrackRequestRepository
    {
        Task<List<TrackRequest>> GetPendingByRestaurantIdAsync(int restaurantId);

        Task<TrackRequest?> GetByIdAsync(Guid requestId);

        Task UpdateAsync(TrackRequest request);
        Task SaveChangesAsync();

    }
}
