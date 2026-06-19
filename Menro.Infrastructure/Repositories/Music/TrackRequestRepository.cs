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

        public async Task<TrackRequest?> GetByIdAsync(Guid requestId)
        {
            return await _context.TrackRequests.FirstOrDefaultAsync(x => x.Id == requestId);
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
