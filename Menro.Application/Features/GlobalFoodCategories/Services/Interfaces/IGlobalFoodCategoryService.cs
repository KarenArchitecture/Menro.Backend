using Menro.Application.Features.GlobalFoodCategories.DTOs;

namespace Menro.Application.Features.GlobalFoodCategories.Services.Interfaces
{
    public interface IGlobalFoodCategoryService
    {
        Task<bool> AddGlobalCategoryAsync(CreateGlobalCategoryDTO dto);
        Task<List<GetGlobalCategoryDTO>> GetAllGlobalCategoriesAsync();
        Task<GetGlobalCategoryDTO> GetGlobalCategoryAsync(int Id);
        Task<bool> UpdateGlobalCategoryAsync(UpdateGlobalCategoryDto dto);
        Task<bool> DeleteGlobalCategoryAsync(int id);
    }
}
