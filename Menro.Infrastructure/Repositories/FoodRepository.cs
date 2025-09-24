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

        //Home Page
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

        //Restaurant Page
        public async Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return new List<Food>();

            return await _context.Foods
                .AsNoTracking()
                .Include(f => f.FoodCategory)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)                 
                    .ThenInclude(r => r.RestaurantCategory)
                .Where(f =>
                    f.Restaurant.Slug == slug &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.IsApproved)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
        
        
        // CRUD
        public async Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId)
        {
            return await _context.Foods
                .Where(u => u.IsDeleted == false && u.IsAvailable == true && u.RestaurantId == restaurantId)
                .Include(f => f.FoodCategory).ToListAsync();
        }
        public async Task<bool> AddFoodAsync(Food food)
        {
            try
            {
                await _context.Foods.AddAsync(food);
                await _context.SaveAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        public async Task<Food> GetFoodDetailsAsync(int foodId)
        {
            var food = await _context.Foods
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)   // ⬅️ Addons از داخل Variant لود میشه
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food is null)
                throw new Exception("food does not exist");

            return food;
        }
        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null)
            {
                return false; // یعنی پیدا نشد
            }

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }





    }
}
