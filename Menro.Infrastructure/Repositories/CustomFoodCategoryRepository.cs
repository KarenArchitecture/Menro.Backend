using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for managing custom food categories belonging to restaurants.
    /// </summary>
    public class CustomFoodCategoryRepository : ICustomFoodCategoryRepository
    {
        private readonly MenroDbContext _context;
        private readonly IMemoryCache _cache;

        public CustomFoodCategoryRepository(MenroDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /* ============================================================
           🔹 Basic CRUD
        ============================================================ */

        /// <summary>
        /// Creates a new custom category record.
        /// </summary>
        public async Task<bool> CreateAsync(CustomFoodCategory category)
        {
            try
            {
                await _context.CustomFoodCategories.AddAsync(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns all active and available categories for a specific restaurant.
        /// </summary>
        public async Task<IEnumerable<CustomFoodCategory>> GetAllAsync(int restaurantId)
        {
            return await _context.CustomFoodCategories
                .AsNoTracking()
                .Include(c => c.Icon)
                .Where(c => c.RestaurantId == restaurantId && !c.IsDeleted && c.IsAvailable)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a single category by its ID (throws if not found).
        /// </summary>
        public async Task<CustomFoodCategory> GetByIdAsync(int catId)
        {
            var category = await _context.CustomFoodCategories
                .AsNoTracking()
                .Include(c => c.Icon)
                .FirstOrDefaultAsync(c => c.Id == catId && !c.IsDeleted);

            if (category == null)
                throw new Exception($"Custom category with ID {catId} not found.");

            return category;
        }

        /// <summary>
        /// Returns a category by its name.
        /// </summary>
        public async Task<CustomFoodCategory?> GetByNameAsync(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.RestaurantId == restaurantId &&
                    c.Name == catName);
        }

        /// <summary>
        /// Returns categories belonging to a restaurant (by slug).
        /// </summary>
        public async Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug)
        {
            return await _context.CustomFoodCategories
                .AsNoTracking()
                .Where(fc => fc.Restaurant.Slug == restaurantSlug)
                .ToListAsync();
        }

        /* ============================================================
           🟢 Public Query for Shop (Section 2: Filter Row)
           - Entities only (no DTOs in Domain)
           - Visible categories for a given restaurant slug
           - Includes Icon and GlobalCategory.Icon (for Application mapping)
           - AsNoTracking + single round-trip
           - Small per-slug cache (5 minutes)
        ============================================================ */

        public async Task<List<CustomFoodCategory>> GetActiveByRestaurantSlug_WithIconsAsync(
    string restaurantSlug,
    CancellationToken ct = default)
        {
            const int cacheDurationMinutes = 5;
            var cacheKey = $"RestaurantCategories_{restaurantSlug}"; // 🔹 same pattern as other repos

            if (_cache.TryGetValue(cacheKey, out List<CustomFoodCategory>? cached))
                return cached;

            var data = await _context.CustomFoodCategories
                .AsNoTracking()
                .Include(c => c.Icon)
                .Include(c => c.GlobalCategory).ThenInclude(g => g.Icon)
                .Where(c => c.Restaurant.Slug == restaurantSlug && !c.IsDeleted && c.IsAvailable)
                .OrderBy(c => c.GlobalCategory != null ? c.GlobalCategory.DisplayOrder : int.MaxValue)
                .ThenBy(c => c.Name ?? c.GlobalCategory!.Name)
                .ToListAsync(ct);

            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(cacheDurationMinutes));
            return data;
        }

        /* ============================================================
           🔎 Validation Helpers
        ============================================================ */

        /// <summary>
        /// Checks if a category name already exists.
        /// </summary>
        public async Task<bool> ExistsByNameAsync(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories
                .AnyAsync(c => c.RestaurantId == restaurantId && c.Name == catName);
        }

        /// <summary>
        /// Checks if a category with the same name exists but is soft-deleted.
        /// </summary>
        public async Task<bool> IsSoftDeleted(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories
                .AnyAsync(c => c.RestaurantId == restaurantId && c.Name == catName && c.IsDeleted);
        }

        /* ============================================================
           🔄 Update & Delete
        ============================================================ */

        /// <summary>
        /// Soft deletes or removes a category depending on its usage.
        /// </summary>
        public async Task<bool> DeleteAsync(int catId)
        {
            var cat = await _context.CustomFoodCategories
                .Include(c => c.Foods)
                .FirstOrDefaultAsync(c => c.Id == catId);

            if (cat is null)
                return false;

            if (cat.Foods.Count == 0)
                _context.CustomFoodCategories.Remove(cat);
            else
                cat.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates an existing category record.
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(CustomFoodCategory category)
        {
            if (category == null) return false;

            _context.CustomFoodCategories.Update(category);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        /* ============================================================
           🔄 Cache invalidation methods (consistent with other repos)
        ============================================================ */
        public void InvalidateRestaurantCategories(string slug)
            => _cache.Remove($"RestaurantCategories_{slug}");
    }
}
