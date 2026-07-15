using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.Foods.DTOs;

namespace Menro.Application.Features.ShowAll.Services.Interfaces
{
    public interface IPopularFoodsBrowseService
    {
        Task<List<PopularFoodsDto>> GetPopularFoodsGroupsAsync(int groupsCount = 2, int foodsPerGroup = 8);

        Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);

        Task<List<int>> GetAllCategoryIdsAsync();

        // ✅ NEW: View All (cursor-based) for one Global Category
        Task<PagedResultDto<HomeFoodCardDto>> BrowsePopularFoodsByCategoryAsync(
            int categoryId,
            int take = 6,
            string? cursor = null,
            CancellationToken ct = default);
    }
}
