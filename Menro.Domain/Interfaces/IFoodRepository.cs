using Menro.Domain.Entities;

public interface IFoodRepository
{
    //Home Page
    Task<List<FoodCategory>> GetAllCategoriesAsync();
    Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count);
    Task<List<int>> GetAllCategoryIdsAsync();
    Task<List<FoodCategory>> GetAllCategoriesExcludingAsync(List<string> excludeTitles);
    Task<List<Food>> GetPopularFoodsByCategoryIdAsync(int categoryId, int take);

    //Restaurant Page
    //Task<List<Food>> GetFoodsByRestaurantIdAsync(int restaurantId);
    //Task<List<Food>> GetFoodsByRestaurantSlugAsync(string slug);
    Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug);
}
