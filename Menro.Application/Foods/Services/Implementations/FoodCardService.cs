using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Implementations
{
    public class FoodCardService : IFoodCardService
    {
        private readonly IFoodRepository _foodRepository;

        public FoodCardService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        public async Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryAsync(int count = 8)
        {
            var categories = await _foodRepository.GetAllCategoriesAsync();

            if (categories == null || !categories.Any())
                return null;

            var random = new Random();
            var randomCategory = categories[random.Next(categories.Count)];

            var foods = await _foodRepository.GetPopularFoodsByCategoryAsync(randomCategory.Id, count);

            var foodDtos = foods.Select(f =>
            {
                var avgRating = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0;
                var voters = f.Ratings.Count;

                return new FoodCardDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Ingredients = f.Ingredients,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    Rating = avgRating,
                    Voters = voters,
                    RestaurantName = f.Restaurant?.Name ?? "",
                    RestaurantCategory = f.Restaurant?.RestaurantCategory?.Name ?? ""
                };
            }).ToList();

            return new PopularFoodCategoryDto
            {
                CategoryTitle = randomCategory.Name,
                Foods = foodDtos
            };
        }

        public async Task<List<FoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            var foods = await _foodRepository.GetPopularFoodsByCategoryAsync(categoryId, count);

            return foods.Select(f =>
            {
                var avgRating = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0;
                var voters = f.Ratings.Count;

                return new FoodCardDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Ingredients = f.Ingredients,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    Rating = avgRating,
                    Voters = voters,
                    RestaurantName = f.Restaurant?.Name ?? "",
                    RestaurantCategory = f.Restaurant?.RestaurantCategory?.Name ?? ""
                };
            }).ToList();
        }

        public async Task<List<int>> GetAllCategoryIdsAsync()
        {
            return await _foodRepository.GetAllCategoryIdsAsync();
        }

        public async Task<PopularFoodCategoryDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles)
        {
            var remainingCategories = await _foodRepository.GetAllCategoriesExcludingAsync(excludeCategoryTitles);
            if (!remainingCategories.Any())
                return null;

            var randomCategory = remainingCategories.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByCategoryAsync(randomCategory.Id, 8);

            var foodDtos = foods.Select(f =>
            {
                var avgRating = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0;
                var voters = f.Ratings.Count;

                return new FoodCardDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Ingredients = f.Ingredients,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    Rating = avgRating,
                    Voters = voters,
                    RestaurantName = f.Restaurant?.Name ?? "",
                    RestaurantCategory = f.Restaurant?.RestaurantCategory?.Name ?? ""
                };
            }).ToList();

            return new PopularFoodCategoryDto
            {
                CategoryTitle = randomCategory.Name,
                Foods = foodDtos
            };
        }
    }
}
