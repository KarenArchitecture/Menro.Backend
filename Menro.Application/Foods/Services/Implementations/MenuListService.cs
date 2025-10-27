using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Implementations
{
    public class MenuListService : IMenuListService
    {
        private readonly IFoodRepository _foodRepository;

        public MenuListService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        //public async Task<List<MenuListFoodDto>> GetMenuListAsync(
        //    int restaurantId, int? globalCategoryId = null, int? customCategoryId = null)
        //{
        //    var foods = await _foodRepository.GetFoodsByRestaurantAsync(restaurantId, globalCategoryId, customCategoryId);

        //    return foods.Select(f => new MenuListFoodDto
        //    {
        //        Id = f.Id,
        //        Name = f.Name,
        //        Price = f.Price,
        //        ImageUrl = f.ImageUrl,
        //        GlobalFoodCategoryName = f.GlobalFoodCategory?.Name,
        //        CustomFoodCategoryName = f.CustomFoodCategory?.Name,
        //        IsAvailable = f.IsAvailable
        //    }).ToList();
        //}

        public async Task<List<MenuListFoodDto>> GetMenuListBySlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null)
        {
            var foods = await _foodRepository.GetFoodsByRestaurantSlugAsync(
                restaurantSlug, globalCategoryId, customCategoryId);

            return foods.Select(f => new MenuListFoodDto
            {
                Id = f.Id,
                Name = f.Name,
                Price = f.Price,
                ImageUrl = f.ImageUrl,
                GlobalFoodCategoryId = f.GlobalFoodCategoryId,
                GlobalFoodCategoryName = f.GlobalFoodCategory?.Name,
                GlobalFoodCategorySvg = f.GlobalFoodCategory?.IconId,
                CustomFoodCategoryId = f.CustomFoodCategoryId,
                CustomFoodCategoryName = f.CustomFoodCategory?.Name,
                CustomFoodCategorySvg = f.CustomFoodCategory?.IconId,
                IsAvailable = f.IsAvailable,
                AverageRating = f.AverageRating,
                VotersCount = f.VotersCount
            }).ToList();
        }


    }
}
