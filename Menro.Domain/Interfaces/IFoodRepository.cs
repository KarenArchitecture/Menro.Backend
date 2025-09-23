using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IFoodRepository
    {
        /* Home Page (GLOBAL categories) */
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync();
        Task<List<int>> GetAllGlobalCategoryIdsAsync();
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeTitles);
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count);

        /* Home Page – restaurant-local categories */
        Task<List<FoodCategory>> GetAllCategoriesAsync();
        Task<List<int>> GetAllCategoryIdsAsync();
        Task<List<FoodCategory>> GetAllCategoriesExcludingAsync(List<string> excludeTitles);
        Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count);
        Task<List<Food>> GetPopularFoodsByCategoryIdAsync(int categoryId, int take);

        /* Restaurant Page */
        Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug);
    }
}
