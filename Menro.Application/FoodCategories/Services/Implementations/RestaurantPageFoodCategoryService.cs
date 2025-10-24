using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    public class RestaurantPageFoodCategoryService : IRestaurantPageFoodCategoryService
    {
        private readonly IFoodCategoryRepository _foodCategoryRepository;
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantPageFoodCategoryService(
            IFoodCategoryRepository foodCategoryRepository,
            IRestaurantRepository restaurantRepository)
        {
            _foodCategoryRepository = foodCategoryRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<List<RestaurantFoodCategoryDto>> GetCategoriesByRestaurantSlugAsync(string restaurantSlug)
        {
            // 1️⃣ Get restaurant by slug (reuse existing method)
            var restaurant = await _restaurantRepository.GetRestaurantBannerBySlugAsync(restaurantSlug);
            if (restaurant == null)
                return new List<RestaurantFoodCategoryDto>();

            // 2️⃣ Get global categories
            var globalCategories = await _foodCategoryRepository.GetActiveGlobalCategoriesAsync();

            // 3️⃣ Get restaurant-specific categories
            var customCategories = await _foodCategoryRepository
                .GetAvailableCustomCategoriesForRestaurantAsync(restaurant.Id);

            // 4️⃣ Map both to DTO
            var result = globalCategories.Select(gc => new RestaurantFoodCategoryDto
            {
                Id = $"g-{gc.Id}",               // ✅ prefixed unique ID
                Name = gc.Name,
                SvgIcon = gc.SvgIcon,
                IsGlobal = true
            })
            .Concat(customCategories.Select(cc => new RestaurantFoodCategoryDto
            {
                Id = $"c-{cc.Id}",               // ✅ prefixed unique ID
                Name = cc.Name,
                SvgIcon = cc.SvgIcon,
                IsGlobal = false
            }))
            .ToList();

            return result;
        }


    }
}