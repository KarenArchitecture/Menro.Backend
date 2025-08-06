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

        public async Task<IEnumerable<ShopFoodCategoryDto>> GetCategoriesForRestaurantAsync(string restaurantSlug)
        {
            var categories = await _foodCategoryRepository.GetByRestaurantSlugAsync(restaurantSlug);

            return categories.Select(c => new ShopFoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SvgIcon = c.SvgIcon
            });
        }
    }
}
