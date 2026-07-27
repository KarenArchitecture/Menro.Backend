using Menro.Application.Common.Models;
using Menro.Application.Features.GlobalFoodCategories.DTOs;

namespace Menro.Application.Features.GlobalFoodCategories.Services
{
    public interface IGlobalFoodCategoryService
    {
        Task<Result> AddGlobalCategoryAsync(CreateGlobalCategoryDTO dto);
        Task<List<GetGlobalCategoryDTO>> GetAllGlobalCategoriesAsync();
        Task<GetGlobalCategoryDTO> GetGlobalCategoryAsync(int Id);
        Task<Result> UpdateGlobalCategoryAsync(UpdateGlobalCategoryDto dto);
        Task<bool> DeleteGlobalCategoryAsync(int id);
    }
}
