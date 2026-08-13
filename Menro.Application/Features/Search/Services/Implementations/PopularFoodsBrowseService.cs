using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Foods.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Application.Features.Search.DTOs;

namespace Menro.Application.Features.Search.Services.Implementations
{
    public class PopularFoodsBrowseService : IPopularFoodsBrowseService
    {
        private readonly IGlobalFoodCategoryRepository _globalCatRepo;
        private readonly IMediaStorageProvider _mediaStorage;

        public PopularFoodsBrowseService(IGlobalFoodCategoryRepository globalCatRepo, IMediaStorageProvider mediaStorage)
        {
            _globalCatRepo = globalCatRepo;
            _mediaStorage = mediaStorage;
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

        private string ResolveCategoryIcon(GlobalFoodCategory category)
        {
            var iconFileName = category.Icon?.FileName;
            return string.IsNullOrEmpty(iconFileName)
                ? string.Empty
                : _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, iconFileName);
        }

        /* ============================================================
           🥇 Main: Get random global categories with popular foods
        ============================================================ */
        public async Task<List<PopularFoodsDto>> GetPopularFoodsGroupsAsync(int groupsCount = 2, int foodsPerGroup = 8)
        {
            var result = new List<PopularFoodsDto>();
            var excludeTitles = new List<string>();
            var eligibleGlobals = await _globalCatRepo.GetEligibleGlobalCategoriesAsync();
            if (eligibleGlobals == null || eligibleGlobals.Count == 0)
                return result;

            var random = new Random();
            var shuffled = eligibleGlobals.OrderBy(_ => random.Next()).ToList();

            foreach (var category in shuffled)
            {
                if (excludeTitles.Contains(category.Name))
                    continue;

                var foods = await _globalCatRepo.GetMostPopularFoodsByGlobalCategoryAsync(category.Id, foodsPerGroup);
                if (foods == null || foods.Count == 0)
                    continue;

                result.Add(new PopularFoodsDto
                {
                    CategoryId = category.Id,
                    CategoryTitle = category.Name,
                    SvgIcon = ResolveCategoryIcon(category),
                    Foods = foods.Select(MapToHomeFoodCardDto).ToList()
                });

                excludeTitles.Add(category.Name);
                if (result.Count >= groupsCount)
                    break;
            }

            return result;
        }

        /* ============================================================
           🎯 Get popular foods for a specific Global Category (small list)
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

        /* ============================================================
           ✅ View All: cursor-based browse for one category
        ============================================================ */
        public async Task<PagedResultDto<HomeFoodCardDto>> BrowsePopularFoodsByCategoryAsync(
            int categoryId,
            int take = 6,
            string? cursor = null,
            CancellationToken ct = default)
        {
            var (foods, nextCursor, hasMore) =
                await _globalCatRepo.BrowsePopularFoodsByGlobalCategoryAsync(categoryId, take, cursor, ct);

            return new PagedResultDto<HomeFoodCardDto>
            {
                Items = foods.Select(MapToHomeFoodCardDto).ToList(),
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }
    }
}