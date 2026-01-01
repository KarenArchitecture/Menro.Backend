using Menro.Domain.Entities;
using Menro.Domain.Enums;
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

        public async Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync()
        {
            var nowActiveGlobals = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .ToListAsync();

            var eligibleGlobalIds = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .Where(f =>
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.Status == RestaurantStatus.Approved &&
                    f.GlobalFoodCategoryId != null)
                .Select(f => f.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return nowActiveGlobals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        public async Task<List<int>> GetAllGlobalCategoryIdsAsync()
        {
            return await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .Select(gc => gc.Id)
                .ToListAsync();
        }

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
                    f.Restaurant.Status == RestaurantStatus.Approved &&
                    f.GlobalFoodCategoryId != null)
                .Select(f => f.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return globals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        public async Task<List<Food>> GetPopularFoodsByGlobalCategoryIdOptimizedAsync(int globalCategoryId, int count = 8)
        {
            return await _context.Foods
                .Where(f =>
                    f.GlobalFoodCategoryId == globalCategoryId &&
                    f.IsAvailable &&
                    !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .Include(f => f.OrderItems)
                .OrderByDescending(f => f.OrderItems.Sum(oi => oi.Quantity))
                .ThenByDescending(f => f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0)
                .ThenByDescending(f => f.Ratings.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug)
        {
            return await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.Restaurant.Slug == slug &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.Status == RestaurantStatus.Approved)
                .Include(f => f.CustomFoodCategory)
                    .ThenInclude(c => c.Icon)
                .Include(f => f.GlobalFoodCategory)
                    .ThenInclude(gc => gc.Icon)
                .Include(f => f.Ratings)
                .Include(f => f.Variants.Where(v => !v.IsDeleted && v.IsAvailable))
                .Include(f => f.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds)
        {
            return await _context.Foods
                .Where(f =>
                    (f.CustomFoodCategoryId != null && categoryIds.Contains(f.CustomFoodCategoryId.Value)) ||
                    (f.GlobalFoodCategoryId != null && categoryIds.Contains(f.GlobalFoodCategoryId.Value)))
                .Where(f => f.IsAvailable && !f.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Food>> GetFoodsByRestaurantSlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null)
        {
            var query = _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.Restaurant.Slug == restaurantSlug &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.Status == RestaurantStatus.Approved)
                .Include(f => f.GlobalFoodCategory)
                .Include(f => f.CustomFoodCategory)
                .AsQueryable();

            if (globalCategoryId.HasValue && customCategoryId.HasValue)
                query = query.Where(f => f.GlobalFoodCategoryId == globalCategoryId || f.CustomFoodCategoryId == customCategoryId);
            else if (globalCategoryId.HasValue)
                query = query.Where(f => f.GlobalFoodCategoryId == globalCategoryId);
            else if (customCategoryId.HasValue)
                query = query.Where(f => f.CustomFoodCategoryId == customCategoryId);

            return await query.ToListAsync();
        }

        public async Task<List<Food>> GetFoodsByRestaurantAsync(
            int restaurantId,
            int? globalCategoryId = null,
            int? customCategoryId = null)
        {
            var query = _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.RestaurantId == restaurantId &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.Status == RestaurantStatus.Approved)
                .Include(f => f.GlobalFoodCategory)
                .Include(f => f.CustomFoodCategory)
                .AsQueryable();

            if (globalCategoryId.HasValue && customCategoryId.HasValue)
                query = query.Where(f => f.GlobalFoodCategoryId == globalCategoryId || f.CustomFoodCategoryId == customCategoryId);
            else if (globalCategoryId.HasValue)
                query = query.Where(f => f.GlobalFoodCategoryId == globalCategoryId);
            else if (customCategoryId.HasValue)
                query = query.Where(f => f.CustomFoodCategoryId == customCategoryId);

            return await query.ToListAsync();
        }

        public async Task<Food?> GetFoodWithVariantsAsync(int foodId)
        {
            return await _context.Foods
                .AsNoTracking()
                .Where(f => f.Id == foodId && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.GlobalFoodCategory)
                .Include(f => f.CustomFoodCategory)
                .Include(f => f.Variants.Where(v => !v.IsDeleted))
                    .ThenInclude(v => v.Addons.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync();
        }

        public async Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId)
        {
            return await _context.Foods
                .Where(f =>
                    f.RestaurantId == restaurantId &&
                    !f.IsDeleted &&
                    f.IsAvailable)
                .Include(f => f.CustomFoodCategory)
                .Include(f => f.Variants.Where(v => !v.IsDeleted))
                .OrderByDescending(f => f.CreatedAt)
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

        /// <summary>
        /// Used for update scenarios: load full graph (including soft-deleted children)
        /// to support restore and prevent duplicates.
        /// </summary>
        public async Task<Food> GetFoodAsync(int foodId)
        {
            var food = await _context.Foods
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food == null)
                throw new Exception("Food does not exist");

            return food;
        }
        public async Task<Food> GetFoodForAdminAsync(int foodId)
        {
            var food = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Variants.Where(v => !v.IsDeleted))
                    .ThenInclude(v => v.Addons.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == foodId && !f.IsDeleted);

            if (food == null)
                throw new Exception("Food does not exist");

            return food;
        }

        public async Task<bool> UpdateFoodAsync(Food food)
        {
            try
            {
                _context.Foods.Update(food);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food == null) return false;

            food.IsDeleted = true;
            food.IsAvailable = false;

            foreach (var v in food.Variants)
            {
                v.IsDeleted = true;
                v.IsAvailable = false;

                foreach (var a in v.Addons)
                    a.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
