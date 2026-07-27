// Application/Features/FoodCombos/Services/Interfaces/IFoodComboService.cs
using Menro.Application.Features.FoodCombos.DTOs;

namespace Menro.Application.Features.FoodCombos.Services.Interfaces
{
    public interface IFoodComboService
    {
        Task<List<int>> GetComboFoodIdsAsync(int foodId, int restaurantId);
        Task<(bool Success, string? Error)> SetCombosAsync(int foodId, List<int> comboFoodIds, int restaurantId);
        Task<List<PublicComboFoodDto>> GetPublicCombosAsync(int foodId);
        Task<Dictionary<int, int>> GetComboCountsAsync(int restaurantId);
    }
}