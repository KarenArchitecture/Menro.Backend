using Menro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IFoodCategoryRepository : IRepository<CustomFoodCategory>
    {

        // == Restaurant Page ==
        /// <summary>
        /// Get all active global food categories (admin-defined) ordered by DisplayOrder.
        /// </summary>
        Task<List<GlobalFoodCategory>> GetActiveGlobalCategoriesAsync();

        /// <summary>
        /// Get all available (not deleted) custom categories for a specific restaurant.
        /// </summary>
        Task<List<CustomFoodCategory>> GetAvailableCustomCategoriesForRestaurantAsync(int restaurantId);

    }
}
