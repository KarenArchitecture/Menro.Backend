using Menro.Application.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Domain.Interfaces;
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

        public async Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId)
        {
            var foods = await _repository.GetFoodsListForAdminAsync(restaurantId);

            var list = foods.Select(f => new FoodsListItemDto
            {
                Id = f.Id,
                Name = f.Name,
                Price = f.Variants.Any() ? 0 : f.Price, // یا هر لاجیک دیگه که داری
                IsAvailable = f.IsAvailable,
                FoodCategoryName = f.CustomFoodCategory.Name
            }).ToList();

            return list;
        }
        public async Task<FoodDetailsDto?> GetFoodAsync(int foodId, int restaurantId)
        {
            var food = await _repository.GetFoodDetailsAsync(foodId);
            return food is null ? null : FoodMapper.MapToDetailsDto(food);
        }

        public async Task<FoodDetailsDto> CreateFoodAsync(CreateFoodDto dto, int restaurantId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var food = new Food
            {
                Name = dto.Name.Trim(),
                Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients) ? null : dto.Ingredients.Trim(),
                Price = dto.HasVariants ? 0 : dto.Price,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                CustomFoodCategoryId = dto.FoodCategoryId,
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

            await _repository.AddFoodAsync(food);

            return FoodMapper.MapToDetailsDto(food);
        }
        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            return await _repository.DeleteFoodAsync(foodId);
        }


    }
}
