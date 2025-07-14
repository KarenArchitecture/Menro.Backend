using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Menro.Infrastructure.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly MenroDbContext _context;

        public FoodRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<List<FoodCategory>> GetAllCategoriesAsync()
        {
            return await _context.FoodCategories.ToListAsync();
        }

        public async Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            return await _context.Foods
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                    .ThenInclude(r => r.RestaurantCategory)
                .Where(f => f.FoodCategoryId == categoryId)
                .OrderByDescending(f => f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0)
                .ThenBy(f => Guid.NewGuid()) // for randomization among same-rated items
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<int>> GetAllCategoryIdsAsync()
        {
            return await _context.FoodCategories
                .Select(fc => fc.Id)
                .ToListAsync();
        }

        public async Task<List<FoodCategory>> GetAllCategoriesExcludingAsync(List<string> excludeTitles)
        {
            return await _context.FoodCategories
                .Where(c => !excludeTitles.Contains(c.Name))
                .ToListAsync();
        }


        public async Task<List<Food>> GetPopularFoodsByCategoryIdAsync(int categoryId, int take)
        {
            return await _context.Foods
                .Include(f => f.Restaurant)
                    .ThenInclude(r => r.RestaurantCategory)
                .Include(f => f.Ratings)
                .Where(f => f.FoodCategoryId == categoryId)
                .OrderByDescending(f => f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0)
                .Take(take)
                .ToListAsync();
        }
    }
}
