using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly MenroDbContext _context;

        public FoodRepository(MenroDbContext context)
        {
            _context = context;
        }

        /* ===================== Home Page (Global categories) ===================== */

        /// <summary>
        /// Get all active global categories that have at least one available food
        /// </summary>
        public async Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync()
        {
            var nowActiveGlobals = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .ToListAsync();

            // Only include global categories that have foods available
            var eligibleGlobalIds = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .Where(f =>
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.IsApproved &&
                    f.GlobalFoodCategoryId != null
                )
                .Select(f => f.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return nowActiveGlobals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        /// <summary>
        /// Get all active global category IDs
        /// </summary>
        public async Task<List<int>> GetAllGlobalCategoryIdsAsync()
        {
            return await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .Select(gc => gc.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active global categories except the excluded titles
        /// </summary>
        public async Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeTitles)
        {
            excludeTitles ??= new List<string>();

            var globals = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive && !excludeTitles.Contains(gc.Name))
                .ToListAsync();

            var eligibleGlobalIds = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .Where(f =>
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.IsApproved &&
                    f.GlobalFoodCategoryId != null
                )
                .Select(f => f.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return globals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        /// <summary>
        /// Get popular foods for a specific global category, ordered by average rating
        /// </summary>
        public async Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count)
        {
            var topIds = await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.GlobalFoodCategoryId == globalCategoryId &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.IsApproved
                )
                .Select(f => new
                {
                    f.Id,
                    Avg = _context.FoodRatings
                            .Where(r => r.FoodId == f.Id)
                            .Select(r => (double?)r.Score)
                            .Average() ?? 0.0,
                    Voters = _context.FoodRatings.Count(r => r.FoodId == f.Id)
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Voters)
                .Take(count)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Foods
                .AsNoTracking()
                .Where(f => topIds.Contains(f.Id))
                .Include(f => f.Restaurant)
                .Include(f => f.Ratings)
                .ToListAsync();
        }

        /* ===================== Restaurant Page ===================== */

        /// <summary>
        /// Get foods by a list of category IDs (custom or global)
        /// </summary>
        public async Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds)
        {
            return await _context.Foods
                .Where(f =>
                    (f.CustomFoodCategoryId != null && categoryIds.Contains(f.CustomFoodCategoryId.Value)) ||
                    (f.GlobalFoodCategoryId != null && categoryIds.Contains(f.GlobalFoodCategoryId.Value))
                )
                .Where(f => f.IsAvailable && !f.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Get the menu for a restaurant by its slug
        /// </summary>
        public async Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return new List<Food>();

            return await _context.Foods
                .AsNoTracking()
                .Include(f => f.CustomFoodCategory)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                    .ThenInclude(r => r.RestaurantCategory)
                .Where(f =>
                    f.Restaurant.Slug == slug &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.IsAvailable && !f.IsDeleted
                )
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        /* ===================== Admin / CRUD ===================== */

        public async Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId)
        {
            return await _context.Foods
                .Include(f => f.CustomFoodCategory)
                .Where(f => f.RestaurantId == restaurantId && !f.IsDeleted && f.IsAvailable)
                .ToListAsync();
        }

        public async Task<bool> AddFoodAsync(Food food)
        {
            try
            {
                await _context.Foods.AddAsync(food);
                await _context.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Food> GetFoodDetailsAsync(int foodId)
        {
            var food = await _context.Foods
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food == null)
                throw new Exception("Food does not exist");

            return food;
        }

        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null) return false;

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}