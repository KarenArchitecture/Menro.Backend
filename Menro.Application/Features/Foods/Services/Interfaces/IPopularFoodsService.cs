using Menro.Application.Features.Foods.DTOs;

namespace Menro.Application.Foods.Services.Interfaces
{
    public interface IPopularFoodsService
    {
        Task<List<PopularFoodsDto>> GetPopularFoodsGroupsAsync(int groupsCount = 2, int foodsPerGroup = 8);

        Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);

        Task<List<int>> GetAllCategoryIdsAsync();
    }
}
