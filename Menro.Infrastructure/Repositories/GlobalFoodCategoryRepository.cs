using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Global Food Categories.
    /// Includes Admin CRUD operations and Home-page queries
    /// with caching and invalidation. Works without requiring
    /// navigation from Global → CustomFoodCategory.
    /// </summary>
    public class GlobalFoodCategoryRepository : IGlobalFoodCategoryRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        private const string EligibleGlobalsCacheKey = "EligibleGlobalCategories";
        private const string PopularFoodsCacheKeyPrefix = "PopularFoods_Category_";

        public GlobalFoodCategoryRepository(MenroDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /* ============================================================
           ⚙️ Admin / Owner Panel (CRUD)
        ============================================================ */

        public async Task<List<GlobalFoodCategory>> GetAllAsync()
            => await _context.GlobalFoodCategories
                .Include(c => c.Icon)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<GlobalFoodCategory> GetByIdAsync(int id)
        {
            var cat = await _context.GlobalFoodCategories
                .Include(g => g.Icon)
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

            if (cat is null)
                throw new Exception("Food category does not exist");

            return cat;
        }

        public async Task<bool> CreateAsync(GlobalFoodCategory category)
        {
            try
            {
                _context.GlobalFoodCategories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(GlobalFoodCategory category)
        {
            if (category == null) return false;
            _context.GlobalFoodCategories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var cat = await _context.GlobalFoodCategories
                .Include(c => c.Foods)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat is null)
                return false;

            if (cat.Foods.Count == 0)
                _context.GlobalFoodCategories.Remove(cat);
            else
                cat.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        /* ============================================================
           🌍 Home Page — Popular Foods Section (with caching)
        ============================================================ */

        /// <summary>
        /// Returns all active global food categories that actually have
        /// available foods (via CustomFoodCategory → Food → Restaurant).
        /// Cached for 10 minutes for performance.
        /// </summary>
        public async Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesAsync()
        {
            if (_cache.TryGetValue(EligibleGlobalsCacheKey, out List<GlobalFoodCategory> cached))
                return cached;

            // Find all GlobalCategory IDs that have at least one active food
            var eligibleIds = await _context.CustomFoodCategories
                .AsNoTracking()
                .Where(cc =>
                    cc.GlobalCategoryId != null &&
                    cc.Foods.Any(f =>
                        f.IsAvailable &&
                        !f.IsDeleted &&
                        f.Restaurant.IsActive &&
                        f.Restaurant.IsApproved))
                .Select(cc => cc.GlobalCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            var result = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc =>
                    gc.IsActive &&
                    !gc.IsDeleted &&
                    eligibleIds.Contains(gc.Id))
                .Include(gc => gc.Icon)
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToListAsync();

            _cache.Set(EligibleGlobalsCacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        /// <summary>
        /// Returns all active global categories except excluded ones.
        /// Cached for 10 minutes.
        /// </summary>
        public async Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesExcludingAsync(List<string> excludeTitles)
        {
            excludeTitles ??= new();
            string cacheKey = $"{EligibleGlobalsCacheKey}_Excluding_{string.Join(',', excludeTitles)}";

            if (_cache.TryGetValue(cacheKey, out List<GlobalFoodCategory> cached))
                return cached;

            var all = await GetEligibleGlobalCategoriesAsync();
            var filtered = all.Where(gc => !excludeTitles.Contains(gc.Name)).ToList();

            _cache.Set(cacheKey, filtered, TimeSpan.FromMinutes(10));
            return filtered;
        }

        /// <summary>
        /// Returns the most popular foods for a global category, cached for 5 minutes.
        /// Popularity = 60% order volume + 30% rating average + 10% voter weight.
        /// </summary>
        public async Task<List<Food>> GetMostPopularFoodsByGlobalCategoryAsync(int globalCategoryId, int count = 8)
        {
            string cacheKey = $"{PopularFoodsCacheKeyPrefix}{globalCategoryId}";
            if (_cache.TryGetValue(cacheKey, out List<Food> cached))
                return cached;

            var foods = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .Include(f => f.OrderItems)
                .Include(f => f.CustomFoodCategory)
                .Where(f =>
                    f.CustomFoodCategory != null &&
                    f.CustomFoodCategory.GlobalCategoryId == globalCategoryId &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.IsApproved)
                .ToListAsync();

            var scored = foods
                .Select(f => new
                {
                    Food = f,
                    Orders = f.OrderItems.Sum(oi => oi.Quantity),
                    Rating = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0.0,
                    Voters = f.Ratings.Count
                })
                .Select(x => new
                {
                    x.Food,
                    Popularity = (x.Orders * 0.6)
                               + (x.Rating * 10 * 0.3)
                               + (Math.Log10(x.Voters + 1) * 10 * 0.1)
                })
                .OrderByDescending(x => x.Popularity)
                .Take(count)
                .Select(x => x.Food)
                .ToList();

            _cache.Set(cacheKey, scored, TimeSpan.FromMinutes(5));
            return scored;
        }

        /* ============================================================
           🔄 Cache Invalidation Helpers
        ============================================================ */

        public void InvalidateGlobalCategoryLists()
            => _cache.Remove(EligibleGlobalsCacheKey);

        public void InvalidatePopularFoodsByCategory(int categoryId)
            => _cache.Remove($"{PopularFoodsCacheKeyPrefix}{categoryId}");
    }
}
