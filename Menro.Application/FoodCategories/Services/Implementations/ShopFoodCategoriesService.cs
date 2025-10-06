using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    public class ShopFoodCategoriesService : IShopFoodCategoriesService
    {
        private readonly IFoodCategoryRepository _foodCategoryRepository;

        public ShopFoodCategoriesService(IFoodCategoryRepository foodCategoryRepository)
        {
            _foodCategoryRepository = foodCategoryRepository;
        }

        public async Task<List<ShopFoodCategoryDto>> GetByRestaurantAsync(int restaurantId)
        {
            var categories = await _foodCategoryRepository.GetAllByRestaurantAsync(restaurantId);

            return categories.Select(c => new ShopFoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SvgIcon = c.SvgIcon
            }).ToList();
        }

        public async Task<List<ShopFoodCategoryDto>> GetGlobalCategoriesAsync()
        {
            var categories = await _foodCategoryRepository.GetGlobalCategoriesAsync();

            return categories.Select(c => new ShopFoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SvgIcon = c.SvgIcon
            }).ToList();
        }

    //    public async Task<List<ShopFoodCategoryDto>> GetAllForRestaurantAsync(string restaurantSlug)
    //    {
    //        var categories = await _foodCategoryRepository.GetAllFoodCategoriesForRestaurantAsync(restaurantSlug);

    //        var categoryIds = categories.Select(c => c.Id).ToList();
    //        var foods = await _foodCategoryRepository.GetByCategoryIdsAsync(categoryIds);

    //        return categories.Select(c => new ShopFoodCategoryDto
    //        {
    //            Id = c.Id,
    //            Name = c.Name,
    //            SvgIcon = c.SvgIcon,
    //            GlobalFoodCategoryId = c.GlobalFoodCategoryId,
    //            Foods = foods.Where(f => f.CategoryId == c.Id).ToList()
    //        }).ToList();
    //    }
    }
}
