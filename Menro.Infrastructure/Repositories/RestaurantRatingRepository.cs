using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRatingRepository : IRestaurantRatingRepository
    {
        private readonly MenroDbContext _context;
        public RestaurantRatingRepository(MenroDbContext context) => _context = context;

        // User must have at least one order from this restaurant that got
        // past Pending and wasn't Cancelled — proof they actually interacted
        // with the restaurant, not just added something to a cart.
        public async Task<bool> UserCanRateRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default)
        {
            return await _context.Orders.AnyAsync(o =>
                o.UserId == userId &&
                o.RestaurantId == restaurantId &&
                o.Status != OrderStatus.Pending &&
                o.Status != OrderStatus.Cancelled, ct);
        }

        public async Task<RestaurantRating?> GetByUserAndRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default)
        {
            return await _context.RestaurantRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RestaurantId == restaurantId, ct);
        }

        public async Task AddAsync(RestaurantRating rating, CancellationToken ct = default)
            => await _context.RestaurantRatings.AddAsync(rating, ct);

        public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct) > 0;

        public async Task<(double Average, int Voters)> GetAggregateAsync(int restaurantId, CancellationToken ct = default)
        {
            var stats = await _context.RestaurantRatings
                .Where(r => r.RestaurantId == restaurantId)
                .GroupBy(_ => 1)
                .Select(g => new { Avg = g.Average(x => (double)x.Score), Count = g.Count() })
                .FirstOrDefaultAsync(ct);

            return stats == null ? (0, 0) : (Math.Round(stats.Avg, 1), stats.Count);
        }
    }
}