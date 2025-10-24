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

        public async Task<FoodDetailsDto> AddFoodAsync(CreateFoodDto dto, int restaurantId)
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
        public async Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId)
        {
            var foods = await _repository.GetFoodsListForAdminAsync(restaurantId);

            return foods.Select(f => new FoodsListItemDto
            {
                Id = f.Id,
                Name = f.Name,
                Price = f.Variants.Any() ? 0 : f.Price,
                IsAvailable = f.IsAvailable,
                FoodCategoryName = f.CustomFoodCategory.Name
            }).ToList();
        }

        // Get food details by id
        public async Task<FoodDetailsDto?> GetFoodAsync(int foodId, int restaurantId)
        {
            var food = await _repository.GetFoodDetailsAsync(foodId);
            return food is null ? null : FoodMapper.MapToDetailsDto(food);
        }
        public async Task<bool> UpdateFoodAsync(UpdateFoodDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));
            var food = await _repository.GetFoodDetailsAsync(dto.Id);
            if (food == null)
                throw new KeyNotFoundException("غذا یافت نشد.");

            // update main fields
            food.Name = dto.Name.Trim();
            food.Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients) ? null : dto.Ingredients.Trim();
            food.ImageUrl = dto.ImageUrl ?? string.Empty;
            food.CustomFoodCategoryId = dto.FoodCategoryId;
            //food.HasVariants = dto.HasVariants;
            food.Price = dto.HasVariants ? 0 : dto.Price;

            // edit or delete variants and addons
            if (dto.HasVariants && dto.Variants != null && dto.Variants.Any())
            {
                // delete removed data
                var variantIds = dto.Variants.Where(v => v.Id != null).Select(v => v.Id!.Value).ToList();
                var variantsToRemove = food.Variants.Where(v => !variantIds.Contains(v.Id)).ToList();
                foreach (var v in variantsToRemove)
                    food.Variants.Remove(v);

                // add or update
                foreach (var vDto in dto.Variants)
                {
                    var existing = food.Variants.FirstOrDefault(v => v.Id == vDto.Id);
                    if (existing == null)
                    {
                        // add
                        food.Variants.Add(new FoodVariant
                        {
                            Name = vDto.Name.Trim(),
                            Price = vDto.Price,
                            Addons = vDto.Addons?.Select(a => new FoodAddon
                            {
                                Name = a.Name.Trim(),
                                ExtraPrice = a.ExtraPrice
                            }).ToList() ?? new List<FoodAddon>()
                        });
                    }
                    else
                    {
                        // update
                        existing.Name = vDto.Name.Trim();
                        existing.Price = vDto.Price;

                        existing.Addons.Clear();
                        foreach (var aDto in vDto.Addons ?? new List<FoodAddonDto>())
                        {
                            existing.Addons.Add(new FoodAddon
                            {
                                Name = aDto.Name.Trim(),
                                ExtraPrice = aDto.ExtraPrice
                            });
                        }
                    }
                }
            }
            else
            {
                food.Variants.Clear();
            }
            return await _repository.UpdateFoodAsync(food);

        }

        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            return await _repository.DeleteFoodAsync(foodId);
        }
    }
}
