using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IGlobalFoodCategoryRepository
    {
        /* ============================================================
           ⚙️ Admin / Owner Panel (CRUD)
        ============================================================ */
        Task<List<GlobalFoodCategory>> GetAllAsync();
        Task<GlobalFoodCategory> GetByIdAsync(int id);
        Task<bool> CreateAsync(GlobalFoodCategory category);
        Task<bool> UpdateCategoryAsync(GlobalFoodCategory category);
        Task<bool> DeleteCategoryAsync(int id);

        /* ============================================================
           🌍 Home Page — Popular Foods Section
        ============================================================ */
        Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesAsync();
        Task<List<GlobalFoodCategory>> GetEligibleGlobalCategoriesExcludingAsync(List<string> excludeTitles);

        Task<List<Food>> GetMostPopularFoodsByGlobalCategoryAsync(int globalCategoryId, int count = 8);

        /* ============================================================
           ✅ View All — cursor-based browse for ONE Global Category
           Returns Foods ordered by popularity (deterministic)
        ============================================================ */
        Task<(List<Food> Items, string? NextCursor, bool HasMore)>
            BrowsePopularFoodsByGlobalCategoryAsync(int globalCategoryId, int take = 6, string? cursor = null, CancellationToken ct = default);

        /* ============================================================
           🔄 Cache Invalidation Helpers
        ============================================================ */
        void InvalidateGlobalCategoryLists();
        void InvalidatePopularFoodsByCategory(int categoryId);
    }
}
