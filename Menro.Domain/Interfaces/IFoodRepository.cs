using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IFoodRepository
    {
        /* Home Page (Popular Foods from random GLOBAL categories) */

        // ?? Get top-rated or most popular foods in a given Global Category
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdOptimizedAsync(int globalCategoryId, int count = 8);

        // ?? Get all global categories (for picking random)
        /* Home Page (Popular Foods from random GLOBAL categories) */
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync();

        // ?? Get all category IDs
        Task<List<int>> GetAllGlobalCategoryIdsAsync();

        // ?? Get all categories except given names
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeCategoryTitles);

                                            // -- Restaurant Page -- //
        Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds);
        Task<List<Food>> GetFoodsByRestaurantSlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null);

        /// <summary>
        /// Get all foods of a restaurant, optionally filtered by global or custom category.
        /// Returns Food entities directly (no DTOs).
        /// </summary>
        Task<List<Food>> GetFoodsByRestaurantAsync(
            int restaurantId,
            int? globalCategoryId = null,
            int? customCategoryId = null);
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeTitles);
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count);

        /// <summary>
        /// Get a single food with its variants and addons for a restaurant.
        /// Returns Food entity directly.
        /// </summary>
        Task<Food?> GetFoodWithVariantsAsync(int foodId);
        /* Restaurant Page */
        Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds);
        Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug);

        // Admin/Owner Panel
        Task<bool> AddFoodAsync(Food food);
        Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId);
        Task<Food> GetFoodDetailsAsync(int foodId);
        Task<bool> UpdateFoodAsync(Food food);
        Task<bool> DeleteFoodAsync(int foodId);
        // Admin/Owner Panel
        Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId);
        Task<bool> AddFoodAsync(Food food);
        Task<Food> GetFoodDetailsAsync(int foodId);
        Task<bool> DeleteFoodAsync (int foodId);
    }
}