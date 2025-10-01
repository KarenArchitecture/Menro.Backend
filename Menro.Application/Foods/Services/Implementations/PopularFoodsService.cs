// Menro.Application/Foods/Services/Implementations/FoodCardService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services.Implementations
{
    public class PopularFoodsService : IPopularFoodsService
    {
        private readonly IFoodRepository _foodRepository;

        public PopularFoodsService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
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
                RestaurantName = f.Restaurant?.Name ?? string.Empty
            };
        }

        public async Task<PopularFoodsDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8)
        {
            // Use GLOBAL categories here
            var globals = await _foodRepository.GetAllGlobalCategoriesAsync();
            if (globals == null || globals.Count == 0) return null;

            var randomGlobal = globals.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdAsync(randomGlobal.Id, count);

            return new PopularFoodsDto
            {
                CategoryTitle = randomGlobal.Name,
                SvgIcon = randomGlobal.SvgIcon,
                Foods = (foods ?? new List<Food>()).Select(MapToHomeFoodCardDto).ToList()
            };
        }

        public async Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            // Ask repository for foods of a specific global category
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdAsync(categoryId, count);

            if (foods == null || foods.Count == 0)
                return new List<HomeFoodCardDto>();

            // Map entity → DTO
            return foods.Select(f =>
            {
                var avg = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0.0;
                return new HomeFoodCardDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ImageUrl = f.ImageUrl ?? string.Empty,
                    Rating = Math.Round(avg, 1),
                    Voters = f.Ratings.Count,
                    RestaurantName = f.Restaurant?.Name ?? string.Empty
                };
            }).ToList();
        }

        public Task<List<int>> GetAllCategoryIdsAsync()
            => _foodRepository.GetAllGlobalCategoryIdsAsync();

        public async Task<PopularFoodsDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles)
        {
            var remaining = await _foodRepository.GetAllGlobalCategoriesExcludingAsync(excludeCategoryTitles ?? new());
            if (remaining == null || remaining.Count == 0) return null;

            var randomGlobal = remaining.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdAsync(randomGlobal.Id, 8);

            return new PopularFoodsDto
            {
                CategoryTitle = randomGlobal.Name,
                SvgIcon = randomGlobal.SvgIcon,
                Foods = (foods ?? new List<Food>()).Select(MapToHomeFoodCardDto).ToList()
            };
        }
    }
}
