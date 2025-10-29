using Menro.Application.Features.GlobalFoodCategories.DTOs;

namespace Menro.Application.Features.GlobalFoodCategories.Services.Interfaces
{
    public interface IGlobalFoodCategoryService
    {
        Task<bool> AddGlobalCategoryAsync(CreateGlobalCategoryDTO dto);
        Task<List<GlobalCategoryDTO>> GetAllGlobalCategoriesAsync();
        Task<GlobalCategoryDTO> GetGlobalCategoryAsync(int Id);
    }
}
