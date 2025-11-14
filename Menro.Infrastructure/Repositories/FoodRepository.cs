using Menro.Application.Foods.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Food entities,
    /// including global category queries, restaurant menus, and admin CRUD.
    /// </summary>
    public class FoodRepository : IFoodRepository
    {
        private readonly MenroDbContext _context;

        public FoodRepository(MenroDbContext context)
        {
            _context = context;
        }

        /* ============================================================
           🔹 Home Page - Global Food Categories
        ============================================================ */

        /// <summary>
        /// Gets all active global categories that have at least one available food.
        /// </summary>
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
                    f.Restaurant.IsApproved &&
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

        /// <summary>
        /// Gets all active global category IDs.
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
        /// Gets all active global categories excluding specific titles.
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

        /// <summary>
        /// Returns most popular foods for a given global category.
        /// </summary>
        public async Task<List<Food>> GetPopularFoodsByGlobalCategoryIdOptimizedAsync(int globalCategoryId, int count = 8)
        {
            return await _context.Foods
                .Where(f => f.GlobalFoodCategoryId == globalCategoryId && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .Include(f => f.OrderItems)
                .OrderByDescending(f => f.OrderItems.Sum(oi => oi.Quantity))
                .ThenByDescending(f => f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0)
                .ThenByDescending(f => f.Ratings.Count)
                .Take(count)
                .ToListAsync();
        }

        /* ============================================================
           🔹 Restaurant Menu Queries
        ============================================================ */

        /// <summary>
        /// Gets foods by custom or global category IDs.
        /// </summary>
        public async Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds)
        {
            return await _context.Foods
                .Where(f =>
                    (f.CustomFoodCategoryId != null && categoryIds.Contains(f.CustomFoodCategoryId.Value)) ||
                    (f.GlobalFoodCategoryId != null && categoryIds.Contains(f.GlobalFoodCategoryId.Value)))
                .Where(f => f.IsAvailable && !f.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets restaurant menu by slug, optionally filtered by category IDs.
        /// </summary>
        public async Task<List<Food>> GetFoodsByRestaurantSlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null)
        {
            var query = _context.Foods
                .AsNoTracking()
                .Where(f => f.Restaurant.Slug == restaurantSlug && f.IsAvailable && !f.IsDeleted)
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

        /// <summary>
        /// Gets restaurant menu by restaurant ID.
        /// </summary>
        public async Task<List<Food>> GetFoodsByRestaurantAsync(
            int restaurantId,
            int? globalCategoryId = null,
            int? customCategoryId = null)
        {
            var query = _context.Foods
                .AsNoTracking()
                .Where(f => f.RestaurantId == restaurantId && f.IsAvailable && !f.IsDeleted)
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

        /// <summary>
        /// Gets a food along with its variants and addons.
        /// </summary>
        public async Task<Food?> GetFoodWithVariantsAsync(int foodId)
        {
            return await _context.Foods
                .AsNoTracking()
                .Where(f => f.Id == foodId && f.IsAvailable && !f.IsDeleted)
                .Include(f => f.GlobalFoodCategory)
                .Include(f => f.CustomFoodCategory)
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                .FirstOrDefaultAsync();
        }

        /* ============================================================
           ⚙️ Admin / CRUD Operations
        ============================================================ */

        /// <summary>
        /// Returns list of foods for admin panel.
        /// </summary>
        public async Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId)
        {
            return await _context.Foods
                .Where(f => f.RestaurantId == restaurantId
                         && !f.IsDeleted
                         && f.IsAvailable)
                .Include(f => f.CustomFoodCategory)
                .Include(f => f.Variants)   // لازم داریم فقط برای قیمت پیش‌فرض
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
        /// <summary>
        /// Adds a new food record.
        /// </summary>
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
        /// Returns full food details including variants and addons.
        /// </summary>
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

        /// <summary>
        /// Updates a food record.
        /// </summary>
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

        /// <summary>
        /// Deletes a food record permanently.
        /// </summary>
        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null) return false;

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count)
        {
            throw new NotImplementedException();
        }

        public Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }
    }
}
