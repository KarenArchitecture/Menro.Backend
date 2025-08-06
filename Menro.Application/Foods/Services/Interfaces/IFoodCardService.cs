using Menro.Application.Foods.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Interfaces
{
    public interface IFoodCardService
    {
        Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8);
        Task<List<FoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);
        Task<List<int>> GetAllCategoryIdsAsync();
        Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles);
        //Task<List<RestaurantMenuDto>> GetFoodsByRestaurantIdAsync(int restaurantId);
    }
}
