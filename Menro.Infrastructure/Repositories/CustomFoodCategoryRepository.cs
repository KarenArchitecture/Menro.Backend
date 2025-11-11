using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for managing custom food categories belonging to restaurants.
    /// </summary>
    public class CustomFoodCategoryRepository : Repository<CustomFoodCategory>, ICustomFoodCategoryRepository
    {
        private readonly MenroDbContext _context;

        public CustomFoodCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
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
            return await _context.CustomFoodCategories.Include(u => u.Icon)
                .Where(u => u.RestaurantId == restaurantId && !u.IsDeleted && u.IsAvailable)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a single category by its ID (throws if not found).
        /// </summary>
        public async Task<CustomFoodCategory> GetByIdAsync(int catId)
        {
            var category = await _context.CustomFoodCategories
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
                .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId && c.Name == catName);
        }

        /// <summary>
        /// Returns categories belonging to a restaurant (by slug).
        /// </summary>
        public async Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug)
        {
            return await _context.CustomFoodCategories
                .Where(fc => fc.Restaurant.Slug == restaurantSlug)
                .ToListAsync();
        }

        /* ============================================================
           ⚙️ Validation Helpers
        ============================================================ */

        /// <summary>
        /// Checks if a category name already exists.
        /// </summary>
        public async Task<bool> ExistsByNameAsync(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories.AnyAsync(u => u.RestaurantId == restaurantId && u.Name == catName);
        }

        /// <summary>
        /// Checks if a category with the same name exists but is soft-deleted.
        /// </summary>
        public async Task<bool> IsSoftDeleted(int restaurantId, string catName)
        {
            var cat = await _context.CustomFoodCategories
                .Where(c => c.IsDeleted && c.RestaurantId == restaurantId)
                .FirstOrDefaultAsync(x => x.Name == catName);

            return cat != null;
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
    }
}
