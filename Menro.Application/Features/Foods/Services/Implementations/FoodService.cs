using Menro.Application.Features.Foods.DTOs;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Foods.Services.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Foods.Services.Implementations
{
    public class FoodService : IFoodService
    {
        #region DI
        private readonly IFoodRepository _repository;
        private readonly ICustomFoodCategoryRepository _cCategoryRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public FoodService(IFoodRepository repository,
            ICustomFoodCategoryRepository cCategoryRepository,
            IMediaStorageProvider mediaStorage)
        {
            _repository = repository;
            _cCategoryRepository = cCategoryRepository;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<bool> AddFoodAsync(CreateFoodDto dto, int restaurantId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            int? gCat = _cCategoryRepository.GetByIdAsync(dto.FoodCategoryId).Result.GlobalCategoryId;

            var food = new Food
            {
                Name = dto.Name.Trim(),
                Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients) ? null : dto.Ingredients.Trim(),
                Price = dto.HasVariants ? 0 : dto.Price,
                ImageUrl = string.Empty,
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

            // مرحله ۱: ساخت رکورد بدون عکس - برای گرفتن Id واقعی (auto-increment)
            var created = await _repository.AddFoodAsync(food);

            if (dto.ImageFile is null || dto.ImageFile.Length == 0)
                return true;

            var entityId = created.Id.ToString();
            try
            {
                var uploadResult = await _mediaStorage.SaveAsync(MediaCategory.RestaurantFoodImage, dto.ImageFile, entityId);
                created.ImageUrl = uploadResult.FileName;
                await _repository.UpdateFoodAsync(created); // اگه اینجا exception بده، catch پایین می‌گیرتش
                return true;
            }
            catch
            {
                // rollback واقعی: غذایی که هیچ‌وقت از دید کاربر "کامل" نبوده، کاملاً حذف بشه (نه soft delete)
                await _repository.RemoveFoodHardAsync(created.Id);
                throw;
            }
        }

        public async Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId)
        {
            var foods = await _repository.GetFoodsListForAdminAsync(restaurantId);

            return foods.Select(f =>
            {
                int displayPrice;
                if (!f.Variants.Any())
                {
                    displayPrice = f.Price;
                }
                else
                {
                    var defaultVariant = f.Variants.FirstOrDefault(v => v.IsDefault == true);
                    displayPrice = defaultVariant?.Price ?? f.Variants.First().Price;
                }

                return new FoodsListItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Price = displayPrice,
                    FoodCategoryName = f.CustomFoodCategory?.Name ?? "بدون دسته‌بندی",
                    IsAvailable = f.IsAvailable,
                    ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, f.Id.ToString(), MediaVariant.Thumbnail),
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
                ImageUrl = string.IsNullOrWhiteSpace(food.ImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, food.ImageUrl, food.Id.ToString(), MediaVariant.Resized),
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

            food.Name = dto.Name.Trim();
            food.Ingredients = string.IsNullOrWhiteSpace(dto.Ingredients)
                ? null
                : dto.Ingredients.Trim();

            // -----------------------------
            // Image: remove / replace / unchanged
            // -----------------------------
            if (dto.RemoveImage)
            {
                if (!string.IsNullOrEmpty(food.ImageUrl))
                    _mediaStorage.Delete(MediaCategory.RestaurantFoodImage, food.ImageUrl, food.Id.ToString());
                food.ImageUrl = string.Empty;
            }
            else if (dto.ImageFile is not null && dto.ImageFile.Length > 0)
            {
                // SaveAsync خودش قبل از نوشتن فایل جدید، نسخه‌ی قدیمی (همه‌ی وریانت‌هاش) رو پاک می‌کنه
                var uploadResult = await _mediaStorage.SaveAsync(
                    MediaCategory.RestaurantFoodImage,
                    dto.ImageFile,
                    food.Id.ToString(),
                    oldFileName: string.IsNullOrEmpty(food.ImageUrl) ? null : food.ImageUrl);

                food.ImageUrl = uploadResult.FileName;
            }
            // else: نه ImageFile اومده نه RemoveImage - عکس فعلی دست‌نخورده می‌مونه

            food.CustomFoodCategoryId = dto.FoodCategoryId;
            food.Price = dto.HasVariants ? 0 : dto.Price;

            // -----------------------------
            // Handle Variants (دست‌نخورده)
            // -----------------------------
            if (!dto.HasVariants)
            {
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

            var dtoVariantIds = dto.Variants
                .Where(v => v.Id.HasValue)
                .Select(v => v.Id!.Value)
                .ToHashSet();

            var variantsToRemove = food.Variants
                .Where(v => !dtoVariantIds.Contains(v.Id) && !v.IsDeleted)
                .ToList();

            foreach (var v in variantsToRemove)
            {
                v.IsDeleted = true;
                v.IsAvailable = false;
                foreach (var a in v.Addons)
                    a.IsDeleted = true;
            }

            foreach (var vDto in dto.Variants)
            {
                var existing = food.Variants.FirstOrDefault(v => v.Id == vDto.Id);

                if (existing == null)
                {
                    var newVariant = new FoodVariant
                    {
                        Name = vDto.Name.Trim(),
                        Price = vDto.Price,
                        IsDefault = vDto.IsDefault,
                        IsDeleted = false,
                        IsAvailable = true,
                        Addons = new List<FoodAddon>()
                    };

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
                else
                {
                    existing.Name = vDto.Name.Trim();
                    existing.Price = vDto.Price;
                    existing.IsDefault = vDto.IsDefault;
                    existing.IsDeleted = false;
                    existing.IsAvailable = true;

                    var dtoAddonIds = (vDto.Addons ?? new List<FoodAddonDto>())
                        .Where(a => a.Id.HasValue)
                        .Select(a => a.Id!.Value)
                        .ToHashSet();

                    var addonsToRemove = existing.Addons
                        .Where(a => !dtoAddonIds.Contains(a.Id) && !a.IsDeleted)
                        .ToList();

                    foreach (var a in addonsToRemove)
                        a.IsDeleted = true;

                    foreach (var aDto in vDto.Addons ?? Enumerable.Empty<FoodAddonDto>())
                    {
                        var existingAddon = existing.Addons.FirstOrDefault(a => a.Id == aDto.Id);

                        if (existingAddon == null)
                        {
                            existing.Addons.Add(new FoodAddon
                            {
                                Name = aDto.Name.Trim(),
                                ExtraPrice = aDto.ExtraPrice,
                                IsDeleted = false
                            });
                        }
                        else
                        {
                            existingAddon.Name = aDto.Name.Trim();
                            existingAddon.ExtraPrice = aDto.ExtraPrice;
                            existingAddon.IsDeleted = false;
                        }
                    }
                }
            }

            return await _repository.UpdateFoodAsync(food);
        }

        public async Task<bool> ToggleFoodStatusAsync(int foodId, int restaurantId)
        {
            var food = await _repository.GetFoodAsync(foodId);
            if (food == null) return false;
            if (food.RestaurantId != restaurantId) return false;

            food.IsAvailable = !food.IsAvailable;
            return await _repository.UpdateFoodAsync(food);
        }

        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            return await _repository.DeleteFoodAsync(foodId);
        }
    }
}