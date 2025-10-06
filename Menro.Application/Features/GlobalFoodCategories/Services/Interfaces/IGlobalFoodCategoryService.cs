using Menro.Application.Features.GlobalFoodCategories.DTOs;

namespace Menro.Application.Features.GlobalFoodCategories.Services.Interfaces
{
    public interface IGlobalFoodCategoryService
    {
        Task<bool> CreateCategoryAsync(CreateGlobalCategoryDTO dto);
        Task<List<GlobalCategoryDTO>> GetAllCategoriesAsync();
        Task<GlobalCategoryDTO> GetGlobalCategoryAsync(int Id);
    }
}
