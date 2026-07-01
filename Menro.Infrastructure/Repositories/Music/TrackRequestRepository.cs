using Menro.Domain.Entities.Music.Enums;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;
using Microsoft.EntityFrameworkCore;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories.Music
{
    public class TrackRequestRepository : ITrackRequestRepository
    {
        private readonly MenroDbContext _context;

        public TrackRequestRepository(MenroDbContext context)
        {
            _context = context;
        }
        public Task AddAsync(TrackRequest request)
        {
            _context.TrackRequests.Add(request);

            return Task.CompletedTask;
        }
        public async Task<List<TrackRequest>> GetAllByMusicTrackId(Guid musicTrackId)
        {
            return await _context.TrackRequests
                .Where(x => x.MusicTrackId == musicTrackId)
                .ToListAsync();
        }
        public async Task<TrackRequest?> GetByIdAsync(Guid requestId)
        {
            return await _context.TrackRequests.FirstOrDefaultAsync(x => x.Id == requestId);
        }
        public async Task<List<TrackRequest>> GetByIdsAsync(List<Guid> requestIds)
        {
            if (requestIds.Count == 0)
                return [];

            return await _context.TrackRequests
                .Where(x => requestIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<List<TrackRequest>> GetPendingByRestaurantIdAsync(int restaurantId)
        {
            return await _context.TrackRequests
                .Include(x => x.MusicTrack)
                .Where(x => x.RestaurantId == restaurantId && x.Status == TrackRequestStatus.Pending)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public Task UpdateAsync(TrackRequest request)
        {
            _context.TrackRequests.Update(request);

            return Task.CompletedTask;
        }
        public Task RemoveRange(IEnumerable<TrackRequest> entity)
        {
            _context.TrackRequests.RemoveRange(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // for public page

        public async Task<List<TrackRequest>> GetTodayByRestaurantAsync(int restaurantId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.TrackRequests
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    x.Status == TrackRequestStatus.Pending &&
                    x.RequestedAt.Date == today)
                .ToListAsync();
        }

        public async Task<bool> HasRequestedTodayAsync(int restaurantId, string userId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.TrackRequests.AnyAsync(x => 
                x.RestaurantId == restaurantId && 
                x.UserId == userId && 
                x.RequestedAt.Date == today);
        }

        public async Task<List<TrackRequest>> GetByMusicTrackIdsAsync(int restaurantId, List<Guid> musicTrackIds)
        {
            if (musicTrackIds == null || musicTrackIds.Count == 0)
                return new List<TrackRequest>();

            return await _context.TrackRequests
                .AsNoTracking()
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    musicTrackIds.Contains(x.MusicTrackId))
                .ToListAsync();
        }
    }
}
