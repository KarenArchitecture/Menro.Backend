using Menro.Application.FoodCategories.DTOs;

namespace Menro.Application.FoodCategories.Services.Interfaces
{
    /// <summary>
    /// Provides restaurant-specific food categories for the public shop page.
    /// Combines both custom and global categories into a unified list.
    /// </summary>
    public interface IRestaurantPageFoodCategoryService
    {
        /// <summary>
        /// Retrieves all active food categories for a restaurant (for the filter row).
        /// </summary>
        Task<List<RestaurantFoodCategoryDto>> GetRestaurantCategoriesAsync(string restaurantSlug, CancellationToken ct = default);
    }
}
