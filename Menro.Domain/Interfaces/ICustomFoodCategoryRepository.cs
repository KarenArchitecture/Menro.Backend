using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface ICustomFoodCategoryRepository : IRepository<CustomFoodCategory>
    {
        Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug);
        Task<bool> CreateAsync(CustomFoodCategory category);
        Task<IEnumerable<CustomFoodCategory>> GetAllAsync(int restaurantId);
        Task<CustomFoodCategory> GetByIdAsync(int catId);
        Task<CustomFoodCategory?> GetByNameAsync(int restaurantId, string catName);
        Task<bool> DeleteAsync(int catId);
        Task<bool> ExistsByNameAsync(int restaurantId, string catName);
        Task<bool> IsSoftDeleted(int restaurantId, string catName);
        Task<bool> UpdateCategoryAsync(CustomFoodCategory category);

    }
}
