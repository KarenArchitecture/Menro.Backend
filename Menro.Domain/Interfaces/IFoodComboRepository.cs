// Domain/Interfaces/IFoodComboRepository.cs
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IFoodComboRepository
    {
        Task<List<int>> GetComboFoodIdsAsync(int foodId);
        Task ReplaceCombosAsync(int foodId, List<int> comboFoodIds);
        Task<List<Food>> GetComboFoodsAsync(int foodId);
        Task<int?> GetRestaurantIdForFoodAsync(int foodId);
        Task<Dictionary<int, int>> GetComboCountsByRestaurantAsync(int restaurantId);
    }
}