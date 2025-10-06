using Menro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IFoodCategoryRepository : IRepository<CustomFoodCategory>
    {
        /// <summary>
        /// Get all custom (restaurant-specific) categories
        /// </summary>
        Task<List<CustomFoodCategory>> GetAllByRestaurantAsync(int restaurantId);

        /// <summary>
        /// Get all global categories
        /// </summary>
        Task<List<GlobalFoodCategory>> GetGlobalCategoriesAsync();

        /// <summary>
        /// Get all food categories (global + restaurant-specific) for a restaurant by its slug
        /// </summary>
        Task<List<object>> GetAllFoodCategoriesForRestaurantAsync(string restaurantSlug);
    }
}
