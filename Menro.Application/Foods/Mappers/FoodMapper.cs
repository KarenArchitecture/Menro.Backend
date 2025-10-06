using Menro.Application.Foods.DTOs;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Mappers
{
    public static class FoodMapper
    {
        public static FoodDetailsDto MapToDetailsDto(Food food)
        {
            return new FoodDetailsDto
            {
                Id = food.Id,
                Name = food.Name,
                Ingredients = food.Ingredients,
                Price = food.Price,
                ImageUrl = food.ImageUrl,
                CustomFoodCategoryId = food.CustomFoodCategoryId,

                Variants = (food.Variants ?? Enumerable.Empty<FoodVariant>())
            .Select(v => new FoodVariantDetailsDto
            {
                Id = v.Id,
                Name = v.Name,
                Price = v.Price,

                // ⬅️ Addons حالا زیر Variant هست
                Addons = (v.Addons ?? Enumerable.Empty<FoodAddon>())
                    .Select(a => new FoodAddonDetailsDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice
                    })
                    .ToList()
            })
            .ToList()
            };
        }
    }
}
