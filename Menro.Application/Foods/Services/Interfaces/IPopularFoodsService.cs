using Menro.Application.Foods.DTOs;
using Menro.Application.Orders.DTOs;

namespace Menro.Application.Foods.Services.Interfaces
{
    /// <summary>
    /// Provides optimized read operations for fetching
    /// "Popular Foods" grouped by Global Categories
    /// for the public homepage and related endpoints.
    /// </summary>
    public interface IPopularFoodsService
    {
        /// <summary>
        /// Returns several random global food categories,
        /// each containing their most popular foods.
        /// </summary>
        /// <param name="groupsCount">Number of categories to include (default 2).</param>
        /// <param name="foodsPerGroup">Number of foods per category (default 8).</param>
        /// <returns>List of grouped popular foods.</returns>
        Task<List<PopularFoodsDto>> GetPopularFoodsGroupsAsync(int groupsCount = 2, int foodsPerGroup = 8);

        /// <summary>
        /// Returns the most popular foods under a specific Global Category,
        /// considering foods that belong indirectly via CustomFoodCategories.
        /// </summary>
        /// <param name="categoryId">GlobalFoodCategory Id.</param>
        /// <param name="count">Number of foods to return (default 8).</param>
        Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8);

        /// <summary>
        /// Returns IDs of all eligible global categories that have active foods.
        /// </summary>
        Task<List<int>> GetAllCategoryIdsAsync();
    }
}
