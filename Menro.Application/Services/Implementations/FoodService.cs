using Menro.Application.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Domain.Entities;
using Menro.Application.Foods.Mappers;

namespace Menro.Application.Services.Implementations
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _repository;

        public FoodService(IFoodRepository repository)
        {
            _repository = repository;
        }

        // List all foods for admin with proper category info
        public async Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId)
        {
            var foods = await _repository.GetFoodsListForAdminAsync(restaurantId);

            return foods.Select(f => new FoodsListItemDto
            {
                Id = f.Id,
                Name = f.Name,
                Price = f.Variants.Any() ? 0 : f.Price,
                IsAvailable = f.IsAvailable,
                FoodCategoryName = f.CustomFoodCategory?.Name ?? f.GlobalFoodCategory?.Name ?? string.Empty,
                FoodCategoryType = f.CustomFoodCategory != null ? "custom" : "global"
            }).ToList();
        }

        // Get food details by id
        public async Task<FoodDetailsDto?> GetFoodAsync(int foodId, int restaurantId)
        {
            var food = await _repository.GetFoodDetailsAsync(foodId);
            return food is null ? null : FoodMapper.MapToDetailsDto(food);
        }

        // Create a new food
        public async Task<FoodDetailsDto> CreateFoodAsync(CreateFoodDto dto, int restaurantId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (dto.CustomFoodCategoryId is null && dto.GlobalFoodCategoryId is null)
                throw new InvalidOperationException("Food must belong to either a custom or a global category.");

            var food = new Food
            {
                Name = dto.Name.Trim(),
                Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients) ? null : dto.Ingredients.Trim(),
                Price = dto.HasVariants ? 0 : dto.Price,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                RestaurantId = restaurantId,
                IsAvailable = true,
                Variants = dto.HasVariants
                    ? dto.Variants!.Select(v => new FoodVariant
                    {
                        Name = v.Name.Trim(),
                        Price = v.Price,
                        Addons = v.Addons?.Select(a => new FoodAddon
                        {
                            Name = a.Name.Trim(),
                            ExtraPrice = a.ExtraPrice
                        }).ToList() ?? new List<FoodAddon>()
                    }).ToList()
                    : new List<FoodVariant>()
            };

            // Assign the proper category
            if (dto.CustomFoodCategoryId.HasValue)
                food.CustomFoodCategoryId = dto.CustomFoodCategoryId.Value;
            else if (dto.GlobalFoodCategoryId.HasValue)
                food.GlobalFoodCategoryId = dto.GlobalFoodCategoryId.Value;

            await _repository.AddFoodAsync(food);

            return FoodMapper.MapToDetailsDto(food);
        }


        // Delete a food
        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            return await _repository.DeleteFoodAsync(foodId);
        }
    }
}
