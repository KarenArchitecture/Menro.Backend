using Menro.Application.Services.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;

namespace Menro.Application.Services.Implementations
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _repository;
        private readonly ICustomFoodCategoryRepository _cCategoryRepository;
        private readonly IFileService _fileService;

        public FoodService(IFoodRepository repository, 
            ICustomFoodCategoryRepository cCategoryRepository,
            IFileService fileService)
        {
            _repository = repository;
            _cCategoryRepository = cCategoryRepository;
            _fileService = fileService;
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
                ImageUrl = dto.ImageName ?? string.Empty,
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
        public async Task<FoodDetailsDto?> GetFoodDetailsAsync(int foodId, int restaurantId)
        {
            var food = await _repository.GetFoodForAdminAsync(foodId);
            if (food == null) return null;

            var dto = new FoodDetailsDto
            {
                Id = food.Id,
                Name = food.Name,
                Ingredients = food.Ingredients,
                Price = food.Price,
                ImageName = food.ImageUrl,
                ImageUrl = food.ImageUrl,
                FoodCategoryId = food.CustomFoodCategoryId!.Value,
                HasVariants = food.Variants.Any(),
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

            return dto;
        }
        public async Task<bool> UpdateFoodAsync(UpdateFoodDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            var food = await _repository.GetFoodAsync(dto.Id);
            if (food == null)
                throw new KeyNotFoundException("غذا یافت نشد.");

            // -----------------------------
            // Update main food fields
            // -----------------------------
            food.Name = dto.Name.Trim();
            food.Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients)
                ? null
                : dto.Ingredients.Trim();


            // new image ?? replace
            if (dto.ImageName != null && food.ImageUrl != dto.ImageName)
            {
                // if there was already an image in food => delete and replace
                if (!string.IsNullOrEmpty(food.ImageUrl))
                    _fileService.DeleteFoodImage(food.ImageUrl);
                food.ImageUrl = dto.ImageName;
            }
            food.CustomFoodCategoryId = dto.FoodCategoryId;
            food.Price = dto.HasVariants ? 0 : dto.Price;

            // -----------------------------
            // Handle Variants
            // -----------------------------
            if (!dto.HasVariants)
            {
                // اگر قبلاً variant داشت → همه را soft delete کن
                foreach (var v in food.Variants)
                {
                    v.IsDeleted = true;
                    v.IsAvailable = false;

                    foreach (var a in v.Addons)
                        a.IsDeleted = true;
                }

                return await _repository.UpdateFoodAsync(food);
            }

            if (dto.Variants is null || !dto.Variants.Any())
                throw new Exception("حداقل یک نوع غذا باید مشخص شود.");


            // --- Step 1: حذف Variant هایی که در dto نیستند (✅ soft delete)
            var dtoVariantIds = dto.Variants
                .Where(v => v.Id.HasValue)
                .Select(v => v.Id!.Value)
                .ToHashSet();

            var variantsToRemove = food.Variants
                .Where(v => !dtoVariantIds.Contains(v.Id) && !v.IsDeleted)
                .ToList(); // ToList برای safe remove

            foreach (var v in variantsToRemove)
            {
                v.IsDeleted = true;
                v.IsAvailable = false;

                foreach (var a in v.Addons)
                    a.IsDeleted = true;
            }


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
                        IsDeleted = false,
                        IsAvailable = true,
                        Addons = new List<FoodAddon>()
                    };

                    // Add Addons
                    foreach (var aDto in vDto.Addons ?? Enumerable.Empty<FoodAddonDto>())
                    {
                        newVariant.Addons.Add(new FoodAddon
                        {
                            Name = aDto.Name.Trim(),
                            ExtraPrice = aDto.ExtraPrice,
                            IsDeleted = false
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

                    existing.IsDeleted = false;   // ✅ restore اگر قبلاً soft delete شده بود
                    existing.IsAvailable = true;

                    // -----------------------------
                    // Sync Addons (Add/Update/Delete)
                    // -----------------------------

                    var dtoAddonIds = (vDto.Addons ?? new List<FoodAddonDto>())
                        .Where(a => a.Id.HasValue)
                        .Select(a => a.Id!.Value)
                        .ToHashSet();

                    // Delete removed addons (✅ soft delete)
                    var addonsToRemove = existing.Addons
                        .Where(a => !dtoAddonIds.Contains(a.Id) && !a.IsDeleted)
                        .ToList();

                    foreach (var a in addonsToRemove)
                        a.IsDeleted = true;

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
                                ExtraPrice = aDto.ExtraPrice,
                                IsDeleted = false
                            });
                        }
                        // update existing addons
                        else
                        {
                            existingAddon.Name = aDto.Name.Trim();
                            existingAddon.ExtraPrice = aDto.ExtraPrice;
                            existingAddon.IsDeleted = false; // ✅ restore
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
