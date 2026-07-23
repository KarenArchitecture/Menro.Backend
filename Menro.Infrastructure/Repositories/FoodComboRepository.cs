// Infrastructure/Repositories/FoodComboRepository.cs
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class FoodComboRepository : IFoodComboRepository
    {
        private readonly MenroDbContext _context;

        public FoodComboRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetComboFoodIdsAsync(int foodId)
        {
            return await _context.FoodCombos
                .AsNoTracking()
                .Where(fc => fc.FoodId == foodId)
                .Select(fc => fc.ComboFoodId)
                .ToListAsync();
        }

        public async Task ReplaceCombosAsync(int foodId, List<int> comboFoodIds)
        {
            var existing = await _context.FoodCombos
                .Where(fc => fc.FoodId == foodId)
                .ToListAsync();

            _context.FoodCombos.RemoveRange(existing);

            var distinctIds = comboFoodIds.Distinct().Where(id => id != foodId).ToList();
            foreach (var comboId in distinctIds)
            {
                _context.FoodCombos.Add(new FoodCombo
                {
                    FoodId = foodId,
                    ComboFoodId = comboId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Food>> GetComboFoodsAsync(int foodId)
        {
            var comboIds = await GetComboFoodIdsAsync(foodId);
            if (comboIds.Count == 0) return new List<Food>();

            return await _context.Foods
                .AsNoTracking()
                .Where(f => comboIds.Contains(f.Id) && f.IsAvailable)
                .Include(f => f.Ratings)
                .Include(f => f.Variants.Where(v => !v.IsDeleted && v.IsAvailable))
                    .ThenInclude(v => v.Addons.Where(a => !a.IsDeleted))
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<int?> GetRestaurantIdForFoodAsync(int foodId)
        {
            return await _context.Foods
                .Where(f => f.Id == foodId)
                .Select(f => (int?)f.RestaurantId)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, int>> GetComboCountsByRestaurantAsync(int restaurantId)
        {
            return await _context.FoodCombos
                .Where(fc => fc.Food.RestaurantId == restaurantId)
                .GroupBy(fc => fc.FoodId)
                .Select(g => new { FoodId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FoodId, x => x.Count);
        }
    }
}