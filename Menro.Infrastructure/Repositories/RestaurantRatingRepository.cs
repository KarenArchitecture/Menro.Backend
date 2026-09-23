using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRatingRepository : Repository<RestaurantRating>, IRestaurantRatingRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantRatingRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<RestaurantRating?> GetByUserAndRestaurantAsync(string userId, int restaurantId)
        {
            return await _context.RestaurantRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RestaurantId == restaurantId);
        }

        public async Task<Dictionary<int, int>> GetUserRatingsForRestaurantsAsync(string userId, List<int> restaurantIds)
        {
            if (string.IsNullOrWhiteSpace(userId) || restaurantIds == null || restaurantIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.RestaurantRatings
                .Where(r => r.UserId == userId && restaurantIds.Contains(r.RestaurantId))
                .ToDictionaryAsync(r => r.RestaurantId, r => r.Score);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}