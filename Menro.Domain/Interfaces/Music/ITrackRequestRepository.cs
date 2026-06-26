using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface ITrackRequestRepository
    {
        Task AddAsync(TrackRequest request);
        Task<List<TrackRequest>> GetPendingByRestaurantIdAsync(int restaurantId);

        Task<TrackRequest?> GetByIdAsync(Guid requestId);

        Task UpdateAsync(TrackRequest request);
        Task SaveChangesAsync();

        // for public page

        Task<List<TrackRequest>> GetTodayByRestaurantAsync(int restaurantId);
        Task<bool> HasRequestedTodayAsync(int restaurantId, string userId);

    }
}
