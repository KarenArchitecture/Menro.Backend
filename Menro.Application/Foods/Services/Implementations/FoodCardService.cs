// Menro.Application/Foods/Services/Implementations/FoodCardService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services.Implementations
{
    public class FoodCardService : IFoodCardService
    {
        private readonly IFoodRepository _foodRepository;

        public FoodCardService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        private static RecentOrdersFoodCardDto MapToHome(Food f)
        {
            var avg = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0.0;
            return new RecentOrdersFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.ImageUrl ?? string.Empty,
                Rating = Math.Round(avg, 1),
                Voters = f.Ratings.Count,
                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        public async Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8)
        {
            // Use GLOBAL categories here
            var globals = await _foodRepository.GetAllGlobalCategoriesAsync();
            if (globals == null || globals.Count == 0) return null;

            var randomGlobal = globals.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdAsync(randomGlobal.Id, count);

            return new PopularFoodCategoryDto
            {
                CategoryTitle = randomGlobal.Name,
                SvgIcon = randomGlobal.SvgIcon,
                Foods = (foods ?? new List<Food>()).Select(MapToHome).ToList()
            };
        }

        public async Task<List<RecentOrdersFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            // Kept for other use-cases; not used by homepage row anymore
            var foods = await _foodRepository.GetPopularFoodsByCategoryAsync(categoryId, count);
            return (foods ?? new List<Food>()).Select(MapToHome).ToList();
        }

        public Task<List<int>> GetAllCategoryIdsAsync()
            => _foodRepository.GetAllGlobalCategoryIdsAsync();

        public async Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles)
        {
            var remaining = await _foodRepository.GetAllGlobalCategoriesExcludingAsync(excludeCategoryTitles ?? new());
            if (remaining == null || remaining.Count == 0) return null;

            var randomGlobal = remaining.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdAsync(randomGlobal.Id, 8);

            return new PopularFoodCategoryDto
            {
                CategoryTitle = randomGlobal.Name,
                SvgIcon = randomGlobal.SvgIcon,
                Foods = (foods ?? new List<Food>()).Select(MapToHome).ToList()
            };
        }
    }
}
