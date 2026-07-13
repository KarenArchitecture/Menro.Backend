// Infrastructure/Repositories/FoodRatingRepository.cs
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class FoodRatingRepository : Repository<FoodRating>, IFoodRatingRepository
    {
        private readonly MenroDbContext _context;

        public FoodRatingRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FoodRating?> GetByFoodAndUserAsync(int foodId, string userId)
        {
            return await _context.FoodRatings
                .FirstOrDefaultAsync(r => r.FoodId == foodId && r.UserId == userId);
        }

        public async Task AddAsync(FoodRating rating)
        {
            await _context.FoodRatings.AddAsync(rating);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}