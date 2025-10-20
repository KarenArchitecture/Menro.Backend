using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface ICustomFoodCategoryRepository : IRepository<CustomFoodCategory>
    {
        Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug);
        Task<bool> CreateAsync(CustomFoodCategory category);
        Task<IEnumerable<CustomFoodCategory>> GetCustomFoodCategoriesAsync(int restaurantId);
        Task<CustomFoodCategory> GetCategoryAsync(int catId);
        Task<bool> DeleteCustomCategoryAsync(int catId);
        Task<bool> ExistsByNameAsync(int restaurantId, string catName);
        Task<bool> UpdateCategoryAsync(CustomFoodCategory category);

    }
}
