// Menro.Application/Foods/Services/Interfaces/IFoodCardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Application.Foods.DTOs;
using Menro.Application.Orders.DTOs;

namespace Menro.Application.Foods.Services.Interfaces
{
    public interface IPopularFoodsService
    {
        Task<PopularFoodsDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8);
        Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);
        Task<List<int>> GetAllCategoryIdsAsync();
        Task<PopularFoodsDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles);
    }
}
