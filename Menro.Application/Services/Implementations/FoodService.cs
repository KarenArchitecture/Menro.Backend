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
        private readonly ICustomFoodCategoryRepository _cCategoryRepository;

        public FoodService(IFoodRepository repository, ICustomFoodCategoryRepository cCategoryRepository)
        {
            _repository = repository;
            _cCategoryRepository = cCategoryRepository;
        }

        public async Task<bool> AddFoodAsync(CreateFoodDto dto, int restaurantId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            int? gCat = _cCategoryRepository.GetByIdAsync(dto.FoodCategoryId).Result.GlobalCategoryId;
            var food = new Food
            {
                Name = dto.Name.Trim(),
                Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients) ? null : dto.Ingredients.Trim(),
                Price = dto.HasVariants ? 0 : dto.Price,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                CustomFoodCategoryId = dto.FoodCategoryId,
                GlobalFoodCategoryId = gCat,
                RestaurantId = restaurantId,
                IsAvailable = true,

                Variants = dto.HasVariants
                    ? dto.Variants!.Select(v => new FoodVariant
                    {
                        Name = v.Name.Trim(),
                        Price = v.Price,
                        IsDefault = v.IsDefault,

                        Addons = v.Addons?.Select(a => new FoodAddon
                        {
                            Name = a.Name.Trim(),
                            ExtraPrice = a.ExtraPrice
                        }).ToList() ?? new List<FoodAddon>()
                    }).ToList()
                    : new List<FoodVariant>()
            };

            return await _repository.AddFoodAsync(food);

        }
        public async Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId)
        {
            var foods = await _repository.GetFoodsListForAdminAsync(restaurantId);

            return foods.Select(f =>
            {
                // قیمت نمایش داده شده
                int displayPrice;

                if (!f.Variants.Any())
                {
                    displayPrice = f.Price;
                }
                else
                {
                    var defaultVariant = f.Variants
                        .FirstOrDefault(v => v.IsDefault == true);

                    displayPrice = defaultVariant?.Price
                        ?? f.Variants.First().Price; // fallback
                }

                return new FoodsListItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Price = displayPrice,
                    FoodCategoryName = f.CustomFoodCategory!.Name,
                    IsAvailable = f.IsAvailable,
                };

            }).ToList();
        }
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

            // -----------------------------
            // Update main food fields
            // -----------------------------
            food.Name = dto.Name.Trim();
            food.Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients)
                ? null
                : dto.Ingredients.Trim();
            food.ImageUrl = dto.ImageUrl ?? string.Empty;
            food.CustomFoodCategoryId = dto.FoodCategoryId;
            food.Price = dto.HasVariants ? 0 : dto.Price;

            // -----------------------------
            // Handle Variants
            // -----------------------------
            if (!dto.HasVariants)
            {
                // اگر قبلاً variant داشت → همه را پاک کن
                food.Variants.Clear();
                return await _repository.UpdateFoodAsync(food);
            }

            if (dto.Variants is null || !dto.Variants.Any())
                throw new Exception("حداقل یک نوع غذا باید مشخص شود.");


            // --- Step 1: حذف Variant هایی که در dto نیستند
            var dtoVariantIds = dto.Variants
                .Where(v => v.Id.HasValue)
                .Select(v => v.Id!.Value)
                .ToHashSet();

            var variantsToRemove = food.Variants
                .Where(v => !dtoVariantIds.Contains(v.Id))
                .ToList(); // ToList برای safe remove

            foreach (var v in variantsToRemove)
                food.Variants.Remove(v);


            // --- Step 2: Add/Update Variants
            foreach (var vDto in dto.Variants)
            {
                var existing = food.Variants.FirstOrDefault(v => v.Id == vDto.Id);

                // add new variants
                if (existing == null)
                {
                    // --- Add new Variant ---
                    var newVariant = new FoodVariant
                    {
                        Name = vDto.Name.Trim(),
                        Price = vDto.Price,
                        IsDefault = vDto.IsDefault,
                        Addons = new List<FoodAddon>()
                    };

                    // Add Addons
                    foreach (var aDto in vDto.Addons ?? Enumerable.Empty<FoodAddonDto>())
                    {
                        newVariant.Addons.Add(new FoodAddon
                        {
                            Name = aDto.Name.Trim(),
                            ExtraPrice = aDto.ExtraPrice
                        });
                    }

                    food.Variants.Add(newVariant);
                }
                // update existing variants
                else
                {
                    existing.Name = vDto.Name.Trim();
                    existing.Price = vDto.Price;
                    existing.IsDefault = vDto.IsDefault;

                    // -----------------------------
                    // Sync Addons (Add/Update/Delete)
                    // -----------------------------

                    var dtoAddonIds = (vDto.Addons ?? new List<FoodAddonDto>())
                        .Where(a => a.Id.HasValue)
                        .Select(a => a.Id!.Value)
                        .ToHashSet();

                    // Delete removed addons
                    var addonsToRemove = existing.Addons
                        .Where(a => !dtoAddonIds.Contains(a.Id))
                        .ToList();
                    foreach (var a in addonsToRemove)
                        existing.Addons.Remove(a);

                    // Add or update addons
                    foreach (var aDto in vDto.Addons ?? Enumerable.Empty<FoodAddonDto>())
                    {
                        var existingAddon = existing.Addons.FirstOrDefault(a => a.Id == aDto.Id);
                        // add new addons
                        if (existingAddon == null)
                        {
                            existing.Addons.Add(new FoodAddon
                            {
                                Name = aDto.Name.Trim(),
                                ExtraPrice = aDto.ExtraPrice
                            });
                        }
                        // update existing addons
                        else
                        {
                            existingAddon.Name = aDto.Name.Trim();
                            existingAddon.ExtraPrice = aDto.ExtraPrice;
                        }
                    }
                }
            }

            return await _repository.UpdateFoodAsync(food);
        }

        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            return await _repository.DeleteFoodAsync(foodId);
        }
    }
}
