using Menro.Application.Foods.DTOs;

namespace Menro.Application.Services.Interfaces
{
    public interface IFoodService
    {
        Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId);
        Task<FoodDetailsDto?> GetFoodDetailsAsync(int foodId, int restaurantId);
        Task<bool> AddFoodAsync(CreateFoodDto dto, int restaurantId);
        Task<bool> UpdateFoodAsync(UpdateFoodDto dto);
        Task<bool> DeleteFoodAsync(int foodId);

    }
}
