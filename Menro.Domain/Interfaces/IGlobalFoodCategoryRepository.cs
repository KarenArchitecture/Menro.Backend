using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing Global Food Categories.
    /// Includes both Admin CRUD operations and optimized
    /// read operations for the public Home Page.
    /// </summary>
    public interface IGlobalFoodCategoryRepository
    {
        /* ============================================================
           ⚙️ Admin / Owner Panel (CRUD)
        ============================================================ */

        /// <summary>
        /// Returns all global food categories (including inactive or deleted ones),
        /// ordered alphabetically, with icon included.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetAllAsync();

        /// <summary>
        /// Returns a single global food category by its ID.
        /// Throws an exception if not found.
        /// </summary>
        Task<GlobalFoodCategory> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new global food category record.
        /// </summary>
        Task<bool> CreateAsync(GlobalFoodCategory category);

        /// <summary>
        /// Updates an existing global food category.
        /// </summary>
        Task<bool> UpdateCategoryAsync(GlobalFoodCategory category);

        /// <summary>
        /// Deletes a global food category (soft-delete if it has foods).
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id);


        /* ============================================================
           🌍 Home Page — Popular Foods Section
        ============================================================ */

        /// <summary>
        /// Returns all active and non-deleted global food categories
        /// that have at least one available food (via their CustomFoodCategories).
        /// Cached for 10 minutes for performance.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesAsync();

        /// <summary>
        /// Returns all eligible global food categories except those
        /// with the given titles (case-sensitive match).
        /// Cached for 10 minutes for performance.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesExcludingAsync(List<string> excludeTitles);

        /// <summary>
        /// Returns the most popular foods belonging to a specific global category.
        /// Popularity is calculated by a weighted formula:
        /// 0.6 × order volume + 0.3 × average rating + 0.1 × voter count.
        /// Cached for 5 minutes for performance.
        /// </summary>
        /// <param name="globalCategoryId">ID of the GlobalFoodCategory.</param>
        /// <param name="count">Maximum number of foods to return (default: 8).</param>
        Task<List<Food>> GetMostPopularFoodsByGlobalCategoryAsync(int globalCategoryId, int count = 8);


        /* ============================================================
           🔄 Cache Invalidation Helpers
        ============================================================ */

        /// <summary>
        /// Clears cached lists of eligible global categories.
        /// Should be called whenever a global category or its foods change.
        /// </summary>
        void InvalidateGlobalCategoryLists();

        /// <summary>
        /// Clears cached popular foods for a specific global category.
        /// Should be called when any food, rating, or order changes under that category.
        /// </summary>
        void InvalidatePopularFoodsByCategory(int categoryId);
    }
}
