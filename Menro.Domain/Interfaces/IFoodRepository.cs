using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing Food entities,
    /// including popular listings, category queries, and CRUD operations.
    /// </summary>
    public interface IFoodRepository
    {
        /* ============================================================
           🔹 Home Page - Popular Foods by Global Category
        ============================================================ */

        /// <summary>
        /// Returns most popular foods for a specific global category,
        /// ordered by sales volume, rating, and voters.
        /// </summary>
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdOptimizedAsync(int globalCategoryId, int count = 8);

        /// <summary>
        /// Returns all active global categories that contain at least one available food.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync();

        /// <summary>
        /// Returns IDs of all active global categories.
        /// </summary>
        Task<List<int>> GetAllGlobalCategoryIdsAsync();

        /// <summary>
        /// Returns all active global categories except those with excluded titles.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeCategoryTitles);

        /* ============================================================
           🔹 Restaurant Page Queries
        ============================================================ */

        /// <summary>
        /// Returns foods by a list of custom or global category IDs.
        /// </summary>
        Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds);

        /// <summary>
        /// Returns menu items for a restaurant by slug, optionally filtered by global/custom categories.
        /// </summary>
        Task<List<Food>> GetFoodsByRestaurantSlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null);

        /// <summary>
        /// Returns menu items for a restaurant by ID, optionally filtered by category.
        /// </summary>
        Task<List<Food>> GetFoodsByRestaurantAsync(
            int restaurantId,
            int? globalCategoryId = null,
            int? customCategoryId = null);

        /// <summary>
        /// Returns a list of popular foods by global category (non-optimized legacy version).
        /// </summary>
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count);

        /// <summary>
        /// Returns a single food entity with its variants and addons.
        /// </summary>
        Task<Food?> GetFoodWithVariantsAsync(int foodId);

        /// <summary>
        /// Returns a restaurant’s full menu list by slug.
        /// </summary>
        Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug);

        /* ============================================================
           ⚙️ Admin / CRUD Operations
        ============================================================ */

        /// <summary>
        /// Adds a new food entity.
        /// </summary>
        Task<bool> AddFoodAsync(Food food);

        /// <summary>
        /// Returns all available foods for a restaurant (for admin panel).
        /// </summary>
        Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId);

        /// <summary>
        /// Returns detailed information of a food item including variants.
        /// </summary>
        Task<Food> GetFoodDetailsAsync(int foodId);

        /// <summary>
        /// Updates an existing food record.
        /// </summary>
        Task<bool> UpdateFoodAsync(Food food);

        /// <summary>
        /// Deletes a food record permanently.
        /// </summary>
        Task<bool> DeleteFoodAsync(int foodId);
    }
}
