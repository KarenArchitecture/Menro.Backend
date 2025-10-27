// Menro.Application/Foods/Services/Implementations/PopularFoodsService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Orders.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Application.Foods.Services.Implementations
{
    public class PopularFoodsService : IPopularFoodsService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IMemoryCache _cache;

        // Cache configuration
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private const string CacheKeyPrefix = "popular_foods_";

        public PopularFoodsService(IFoodRepository foodRepository, IMemoryCache cache)
        {
            _foodRepository = foodRepository;
            _cache = cache;
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
            const string cacheKey = $"{CacheKeyPrefix}random_category";

            if (_cache.TryGetValue(cacheKey, out PopularFoodsDto cached))
                return cached;

            var globals = await _foodRepository.GetAllGlobalCategoriesAsync();
            if (globals == null || globals.Count == 0)
                return null;

            var randomGlobal = globals.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdOptimizedAsync(randomGlobal.Id, count);

            var result = new PopularFoodsDto
            {
                CategoryTitle = randomGlobal.Name,
                IconId = randomGlobal.IconId,
                Foods = (foods ?? new List<Food>()).Select(MapToHomeFoodCardDto).ToList()
            };

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            });

            return result;
        }

        public async Task<List<HomeFoodCardDto>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            var cacheKey = $"{CacheKeyPrefix}category_{categoryId}";

            if (_cache.TryGetValue(cacheKey, out List<HomeFoodCardDto> cachedFoods))
                return cachedFoods;

            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdOptimizedAsync(categoryId, count);
            var result = foods.Select(MapToHomeFoodCardDto).ToList();

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            });

            return result;
        }

        public Task<List<int>> GetAllCategoryIdsAsync()
            => _foodRepository.GetAllGlobalCategoryIdsAsync();

        public async Task<PopularFoodsDto?> GetPopularFoodsFromRandomCategoryExcludingAsync(List<string> excludeCategoryTitles)
        {
            var key = $"{CacheKeyPrefix}exclude_{string.Join('_', excludeCategoryTitles ?? new())}";

            if (_cache.TryGetValue(key, out PopularFoodsDto cached))
                return cached;

            var remaining = await _foodRepository.GetAllGlobalCategoriesExcludingAsync(excludeCategoryTitles ?? new());
            if (remaining == null || remaining.Count == 0)
                return null;

            var randomGlobal = remaining.OrderBy(_ => Guid.NewGuid()).First();
            var foods = await _foodRepository.GetPopularFoodsByGlobalCategoryIdOptimizedAsync(randomGlobal.Id, 8);

            var result = new PopularFoodsDto
            {
                CategoryTitle = randomGlobal.Name,
                IconId = randomGlobal.IconId,
                Foods = (foods ?? new List<Food>()).Select(MapToHomeFoodCardDto).ToList()
            };

            _cache.Set(key, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            });

            return result;
        }

        // 🔄 Cache Invalidation
        // Call this method whenever foods or their ratings are updated in the system
        public void InvalidatePopularFoodsCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                // Compact(1.0) clears all entries; use sparingly
                memoryCache.Compact(1.0);
            }
        }
    }
}
