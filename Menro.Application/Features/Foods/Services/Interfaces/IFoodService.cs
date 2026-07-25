using Menro.Application.Features.Foods.DTOs;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Foods.Services.Interfaces
{
    public interface IFoodService
    {
        Task<string> UploadFoodImageAsync(IFormFile file);
        Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId);
        Task<FoodDetailsDto?> GetFoodDetailsAsync(int foodId, int restaurantId);
        Task<bool> AddFoodAsync(CreateFoodDto dto, int restaurantId);
        Task<bool> UpdateFoodAsync(UpdateFoodDto dto);
        Task<bool> ToggleFoodStatusAsync(int foodId, int restaurantId);
        Task<bool> DeleteFoodAsync(int foodId);

    }
}
