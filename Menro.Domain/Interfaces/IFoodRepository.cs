using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IFoodRepository
    {
        /* Home Page (Popular Foods from random GLOBAL categories) */
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync();
        Task<List<int>> GetAllGlobalCategoryIdsAsync();
        Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeTitles);
        Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count);

        /* Restaurant Page */
        Task<List<Food>> GetByCategoryIdsAsync(List<int> categoryIds);
        Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug);

        // Admin/Owner Panel
        Task<bool> AddFoodAsync(Food food);
        Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId);
        Task<Food> GetFoodDetailsAsync(int foodId);
        Task<bool> UpdateFoodAsync(Food food);
        Task<bool> DeleteFoodAsync(int foodId);
    }
}