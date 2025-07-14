using Menro.Domain.Entities;

public interface IFoodRepository
{
    Task<List<FoodCategory>> GetAllCategoriesAsync();
    Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count);
    Task<List<int>> GetAllCategoryIdsAsync();
    Task<List<FoodCategory>> GetAllCategoriesExcludingAsync(List<string> excludeTitles);
    Task<List<Food>> GetPopularFoodsByCategoryIdAsync(int categoryId, int take); // ✅ Changed Guid -> int
}
