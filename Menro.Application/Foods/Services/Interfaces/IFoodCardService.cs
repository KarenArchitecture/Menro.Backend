// Menro.Application/Foods/Services/Interfaces/IFoodCardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Application.Orders.DTOs;

namespace Menro.Application.Foods.Services.Interfaces
{
    public interface IFoodCardService
    {
        Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8);
        Task<List<RecentOrdersFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);
        Task<List<int>> GetAllCategoryIdsAsync();
        Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles);
    }
}
