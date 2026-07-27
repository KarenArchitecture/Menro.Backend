using Menro.Application.Common.Models;
using Menro.Application.Features.FoodCategories.DTOs;

namespace Menro.Application.Features.FoodCategories.Services.Interfaces
{
    public interface ICustomFoodCategoryService
    {
        Task<List<FoodCategorySelectListDto>> GetCustomFoodCategoriesAsync(int restaurantId);
        Task<List<GetCustomCategoryDto>> GetAllCustomFoodCategoriesAsync(int restaurantId);
        Task<GetCustomCategoryDto> GetCategoryAsync(int catId);
        Task<Result> AddCategoryAsync(CreateCustomFoodCategoryDto dto, int restaurantId);
        Task<Result> AddFromGlobalAsync(int globalCategoryId, int restaurantId);
        Task<bool> DeleteCustomCategoryAsync(int catId);
        Task<Result> UpdateCategoryAsync(UpdateCustomFoodCategoryDto dto);

    }
}
