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
        Task<List<FoodCategoryDto>> GetCustomFoodCategoriesAsync(int restaurantId);
        //Task<FoodCategoryDto> GetByIdAsync(int foodCategoryId);
        //Task<List<FoodsListItemDto>> GetFoodCategoriesListAsync(int restaurantId);
        //Task<FoodDetailsDto?> GetFoodCategoryAsync(int foodCategoryId, int restaurantId);
        Task<bool> AddCategoryAsync(CreateCustomFoodCategoryDto category);
        Task<bool> AddFromGlobalAsync(int globalCategoryId, int restaurantId);
        Task<bool> DeleteCustomCategoryAsync(int catId);

    }
}
