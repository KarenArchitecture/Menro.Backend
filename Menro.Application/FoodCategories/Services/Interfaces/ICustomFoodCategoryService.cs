using Menro.Application.Features.CustomFoodCategory.DTOs;
using Menro.Application.FoodCategories.DTOs;
using Menro.Application.Foods.DTOs;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Interfaces
{
    public interface ICustomFoodCategoryService
    {
        Task<List<FoodCategorySelectListDto>> GetCustomFoodCategoriesAsync(int restaurantId);
        Task<List<GetCustomCategoryDto>> GetAllCustomFoodCategoriesAsync(int restaurantId);
        Task<GetCustomCategoryDto> GetCategoryAsync(int catId);
        Task<bool> AddCategoryAsync(CreateCustomFoodCategoryDto dto, int restaurantId);
        Task<bool> AddFromGlobalAsync(int globalCategoryId, int restaurantId);
        Task<bool> DeleteCustomCategoryAsync(int catId);
        Task<bool> UpdateCategoryAsync(UpdateCustomFoodCategoryDto dto);

    }
}
