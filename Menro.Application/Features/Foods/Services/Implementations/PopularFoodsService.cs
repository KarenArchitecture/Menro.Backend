using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Foods.Services
{
    public class PopularFoodsService : IPopularFoodsService
    {
        private readonly IGlobalFoodCategoryRepository _globalCatRepo;
        private readonly IMediaStorageProvider _mediaStorage;

        public PopularFoodsService(IGlobalFoodCategoryRepository globalCatRepo, IMediaStorageProvider mediaStorage)
        {
            _globalCatRepo = globalCatRepo;
            _mediaStorage = mediaStorage;
        }

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
                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        private string ResolveCategoryIcon(GlobalFoodCategory category)
        {
            var iconFileName = category.Icon?.FileName;
            return string.IsNullOrEmpty(iconFileName)
                ? string.Empty
                : _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, iconFileName);
        }

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

        public async Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            var foods = await _globalCatRepo.GetMostPopularFoodsByGlobalCategoryAsync(categoryId, count);
            return foods.Select(MapToHomeFoodCardDto).ToList();
        }

        public async Task<List<int>> GetAllCategoryIdsAsync()
        {
            var all = await _globalCatRepo.GetEligibleGlobalCategoriesAsync();
            return all?.Select(x => x.Id).ToList() ?? new List<int>();
        }
    }
}