using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing custom food categories
    /// created by individual restaurants.
    /// </summary>
    public interface ICustomFoodCategoryRepository
    {
        /* ============================================================
           🔹 Retrieval Methods (Admin / Internal)
        ============================================================ */

        /// <summary>
        /// Returns all custom categories belonging to a restaurant by slug.
        /// </summary>
        Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug);

        /// <summary>
        /// Returns all active and available custom categories for a restaurant.
        /// </summary>
        Task<IEnumerable<CustomFoodCategory>> GetAllAsync(int restaurantId);

        /// <summary>
        /// Retrieves a custom category by its ID.
        /// </summary>
        Task<CustomFoodCategory> GetByIdAsync(int catId);

        /// <summary>
        /// Retrieves a custom category by its name.
        /// </summary>
        Task<CustomFoodCategory?> GetByNameAsync(int restaurantId, string catName);

        /* ============================================================
           🟢 Public Query for Shop (Section 2: filter row)
           NOTE: Entities only. Includes the icon navigation needed to map in Application.
        ============================================================ */
        Task<List<CustomFoodCategory>> GetActiveByRestaurantSlug_WithIconsAsync(
            string restaurantSlug,
            CancellationToken ct = default
        );

        /* ============================================================
           ⚙️ CRUD Operations
        ============================================================ */

        /// <summary>
        /// Creates a new custom category.
        /// </summary>
        Task<bool> CreateAsync(CustomFoodCategory category);

        /// <summary>
        /// Updates an existing custom category.
        /// </summary>
        Task<bool> UpdateCategoryAsync(CustomFoodCategory category);

        /// <summary>
        /// Deletes (soft or hard) a custom category.
        /// </summary>
        Task<bool> DeleteAsync(int catId);

        /* ============================================================
           🔎 Validation
        ============================================================ */

        /// <summary>
        /// Checks whether a custom category name already exists.
        /// </summary>
        Task<bool> ExistsByNameAsync(int restaurantId, string catName);

        /// <summary>
        /// Checks whether a category with the same name was previously soft-deleted.
        /// </summary>
        Task<bool> IsSoftDeleted(int restaurantId, string catName);

        /* ============================================================
           🧠 Cache invalidation (for internal use)
        ============================================================ */
        void InvalidateRestaurantCategories(string slug);

    }


}
