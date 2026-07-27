using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
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
                .Where(g => !g.IsDeleted)
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

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.GlobalFoodCategories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Name == name);
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
                .IgnoreQueryFilters()
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

        public async Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesAsync()
        {
            if (_cache.TryGetValue(EligibleGlobalsCacheKey, out List<GlobalFoodCategory> cached))
                return cached;

            var eligibleIds = await _context.CustomFoodCategories
                .AsNoTracking()
                .Where(cc =>
                    cc.GlobalCategoryId != null &&
                    cc.Foods.Any(f =>
                        f.IsAvailable &&
                        !f.IsDeleted &&
                        f.Restaurant.IsActive &&
                        f.Restaurant.Status == RestaurantStatus.Approved))
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
                    f.Restaurant.Status == RestaurantStatus.Approved)
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
           ✅ View All — cursor-based browse for one Global Category
           No new entity/model needed: returns List<Food> directly.
        ============================================================ */
        public async Task<(List<Food> Items, string? NextCursor, bool HasMore)>
        BrowsePopularFoodsByGlobalCategoryAsync(int globalCategoryId, int take = 6, string? cursor = null, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 50);

            var offset = 0;
            if (!string.IsNullOrWhiteSpace(cursor) && int.TryParse(cursor, out var parsed) && parsed > 0)
                offset = parsed;

            // 1) Get ordered IDs only (NO Include)
            var baseFoods = _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.CustomFoodCategory != null &&
                    f.CustomFoodCategory.GlobalCategoryId == globalCategoryId &&
                    f.IsAvailable &&
                    !f.IsDeleted &&
                    f.Restaurant.IsActive &&
                    f.Restaurant.Status == RestaurantStatus.Approved);

            var ordersAgg = _context.OrderItems
                .AsNoTracking()
                .GroupBy(oi => oi.FoodId)
                .Select(g => new { FoodId = g.Key, Orders = g.Sum(x => x.Quantity) });

            var ratingsAgg = _context.Set<FoodRating>()
                .AsNoTracking()
                .GroupBy(r => r.FoodId)
                .Select(g => new { FoodId = g.Key, Avg = g.Average(x => x.Score), Voters = g.Count() });

            var scored = from f in baseFoods.Select(f => new { f.Id })
                         join o in ordersAgg on f.Id equals o.FoodId into og
                         from o in og.DefaultIfEmpty()
                         join r in ratingsAgg on f.Id equals r.FoodId into rg
                         from r in rg.DefaultIfEmpty()
                         select new
                         {
                             Id = f.Id,
                             Orders = o == null ? 0 : o.Orders,
                             Avg = r == null ? 0.0 : (double)r.Avg,
                             Voters = r == null ? 0 : r.Voters,
                             Popularity =
                                ((o == null ? 0 : o.Orders) * 0.6)
                                + ((r == null ? 0.0 : (double)r.Avg) * 10 * 0.3)
                                + (Math.Log10((r == null ? 0 : r.Voters) + 1.0) * 10 * 0.1)
                         };

            var page = await scored
                .OrderByDescending(x => x.Popularity)
                .ThenByDescending(x => x.Id)
                .Skip(offset)
                .Take(take + 1)
                .ToListAsync(ct);

            var hasMore = page.Count > take;
            if (hasMore) page.RemoveAt(page.Count - 1);

            var ids = page.Select(x => x.Id).ToList();

            // 2) Fetch only the 6 foods we need (with Includes for mapping)
            var foods = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                .Where(f => ids.Contains(f.Id))
                .ToListAsync(ct);

            // preserve ordering
            var byId = foods.ToDictionary(x => x.Id);
            var orderedFoods = ids.Where(byId.ContainsKey).Select(id => byId[id]).ToList();

            var nextCursor = hasMore ? (offset + take).ToString() : null;
            return (orderedFoods, nextCursor, hasMore);
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