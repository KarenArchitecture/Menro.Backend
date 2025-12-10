using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services
{
    /// <summary>
    /// High-performance service that provides "Popular Foods"
    /// data for the public home page — built on repository-level caching.
    /// </summary>
    public class PopularFoodsService : IPopularFoodsService
    {
        private readonly IGlobalFoodCategoryRepository _globalCatRepo;

        public PopularFoodsService(IGlobalFoodCategoryRepository globalCatRepo)
        {
            _globalCatRepo = globalCatRepo;
        }

        /* ============================================================
           🧭 Helper: Map Food entity → HomeFoodCardDto
        ============================================================ */
        private static HomeFoodCardDto MapToHomeFoodCardDto(Food f)
        {
            var avg = f.Ratings?.Any() == true ? f.Ratings.Average(r => r.Score) : 0.0;
            return new HomeFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.ImageUrl ?? string.Empty,
                Rating = Math.Round(avg, 1),
                Voters = f.Ratings?.Count ?? 0,
                RestaurantName = f.Restaurant?.Name ?? string.Empty
            };
        }

        /* ============================================================
           🥇 Main: Get random global categories with popular foods
        ============================================================ */
        public async Task<List<PopularFoodsDto>> GetPopularFoodsGroupsAsync(int groupsCount = 2, int foodsPerGroup = 8)
        {
            var result = new List<PopularFoodsDto>();
            var excludeTitles = new List<string>();

            // 1️⃣ Fetch all eligible global categories (active + has foods via custom/global)
            var eligibleGlobals = await _globalCatRepo.GetEligibleGlobalCategoriesAsync();
            if (eligibleGlobals == null || eligibleGlobals.Count == 0)
                return result;

            // 2️⃣ Shuffle them to get random order
            var random = new Random();
            var shuffled = eligibleGlobals.OrderBy(_ => random.Next()).ToList();

            // 3️⃣ Pick random categories until we fill required groups
            foreach (var category in shuffled)
            {
                if (excludeTitles.Contains(category.Name))
                    continue;

                // 4️⃣ Get most popular foods (via CustomFoodCategory → Foods)
                var foods = await _globalCatRepo.GetMostPopularFoodsByGlobalCategoryAsync(category.Id, foodsPerGroup);
                if (foods == null || foods.Count == 0)
                    continue;

                result.Add(new PopularFoodsDto
                {
                    CategoryTitle = category.Name,
                    IconId = category.IconId,
                    Foods = foods.Select(MapToHomeFoodCardDto).ToList()
                });

                excludeTitles.Add(category.Name);
                if (result.Count >= groupsCount)
                    break;
            }

            return result;
        }

        /* ============================================================
           🎯 Get popular foods for a specific Global Category
        ============================================================ */
        public async Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            var foods = await _globalCatRepo.GetMostPopularFoodsByGlobalCategoryAsync(categoryId, count);
            return foods.Select(MapToHomeFoodCardDto).ToList();
        }

        /* ============================================================
           🧾 Get all global category IDs (helper)
        ============================================================ */
        public async Task<List<int>> GetAllCategoryIdsAsync()
        {
            var all = await _globalCatRepo.GetEligibleGlobalCategoriesAsync();
            return all?.Select(x => x.Id).ToList() ?? new List<int>();
        }
    }
}
