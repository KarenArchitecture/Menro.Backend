using Menro.Application.Foods.DTOs;
using Menro.Domain.Entities;

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

                // keep raw here if this mapper is used internally
                ImageUrl = food.ImageUrl,
                ImageName = food.ImageUrl,

                FoodCategoryId = food.CustomFoodCategoryId ?? food.GlobalFoodCategoryId,

                HasVariants = food.Variants?.Any() == true,

                Variants = (food.Variants ?? Enumerable.Empty<FoodVariant>())
                    .Select(v => new FoodVariantDetailsDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Price = v.Price,
                        IsDefault = v.IsDefault,
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
