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
    public class MenuItemService : IMenuItemService
    {
        private readonly IFoodRepository _foodRepository;

        public MenuItemService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }


        public async Task<MenuFoodDetailDto?> GetMenuItemDetailAsync(int foodId)
        {
            var food = await _foodRepository.GetFoodWithVariantsAsync(foodId);
            if (food == null) return null;

            return new MenuFoodDetailDto
            {
                Id = food.Id,
                Name = food.Name,
                ImageUrl = food.ImageUrl,
                Price = food.Price,
                Ingredients = food.Ingredients,
                IsAvailable = food.IsAvailable,
                AverageRating = food.Ratings != null && food.Ratings.Any() ? food.Ratings.Average(r => r.Score) : 0,
                VotersCount = food.Ratings?.Count ?? 0,

                CustomFoodCategoryId = food.CustomFoodCategoryId,
                CustomFoodCategoryName = food.CustomFoodCategory?.Name,
                CustomFoodCategorySvg = food.CustomFoodCategory?.SvgIcon,

                GlobalFoodCategoryId = food.GlobalFoodCategoryId,
                GlobalFoodCategoryName = food.GlobalFoodCategory?.Name,
                GlobalFoodCategorySvg = food.GlobalFoodCategory?.SvgIcon,

                Variants = food.Variants?.Select(v => new MenuFoodVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    IsAvailable = v.IsAvailable,
                    Addons = v.Addons?.Select(a => new MenuFoodAddonDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice
                    }).ToList() ?? new List<MenuFoodAddonDto>()
                }).ToList() ?? new List<MenuFoodVariantDto>()
            };
        }
    }
}
