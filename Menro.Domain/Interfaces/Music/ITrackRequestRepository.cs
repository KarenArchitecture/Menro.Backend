using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore;

namespace Menro.Domain.Interfaces.Music
{
    public interface ITrackRequestRepository
    {
        Task AddAsync(TrackRequest request);
        Task<List<TrackRequest>> GetPendingByRestaurantIdAsync(int restaurantId);
        Task<List<TrackRequest>> GetAllByMusicTrackId(Guid musicTrackId);
        Task<TrackRequest?> GetByIdAsync(Guid requestId);
        Task<List<TrackRequest>> GetByIdsAsync(List<Guid> requestIds);

        Task UpdateAsync(TrackRequest request);
        Task RemoveRange(IEnumerable<TrackRequest> entity);
        Task SaveChangesAsync();

        // for public page

        Task<List<TrackRequest>> GetTodayByRestaurantAsync(int restaurantId);
        Task<bool> HasRequestedTodayAsync(int restaurantId, string userId);
        Task<List<TrackRequest>> GetByMusicTrackIdsAsync(int restaurantId, List<Guid> musicTrackIds);

    }
}
