using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IFoodCategoryRepository : IRepository<FoodCategory>
    {
        Task<List<FoodCategory>> GetAllByRestaurantAsync(int restaurantId);

        Task<List<FoodCategory>> GetGlobalCategoriesAsync();

        /// <summary>
        /// Get all food categories (global + restaurant-specific) for a restaurant by its slug
        /// </summary>
        Task<List<FoodCategory>> GetAllFoodCategoriesForRestaurantAsync(string restaurantSlug);
    }
}
