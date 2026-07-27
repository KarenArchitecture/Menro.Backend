using Menro.Application.Features.FoodCategories.DTOs;
using Menro.Application.Features.FoodCategories.Services.Interfaces;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.Models;

namespace Menro.Application.Features.FoodCategories.Services.Implementations
{
    public class CustomFoodCategoryService : ICustomFoodCategoryService
    {
        #region DI
        private readonly ICustomFoodCategoryRepository _cCatRepository;
        private readonly IGlobalFoodCategoryRepository _gCatRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public CustomFoodCategoryService(
            ICustomFoodCategoryRepository cCatRepository,
            IGlobalFoodCategoryRepository gCatRepository,
            IMediaStorageProvider mediaStorage)
        {
            _cCatRepository = cCatRepository;
            _gCatRepository = gCatRepository;
            _mediaStorage = mediaStorage;
        }
        #endregion
        public async Task<Result> AddCategoryAsync(CreateCustomFoodCategoryDto dto, int restaurantId)
        {
            if (dto is null || restaurantId == 0)
                return Result.Failure("اطلاعات ارسالی نامعتبر است.", ErrorCode.Invalid);

            var name = (dto.Name ?? string.Empty).Trim();

            if (await _cCatRepository.ExistsByNameAsync(restaurantId, name))
            {
                if (await _cCatRepository.IsSoftDeleted(restaurantId, name))
                {
                    var sDeletedCat = await _cCatRepository.GetByNameAsync(restaurantId, name);
                    sDeletedCat.IsDeleted = false;
                    sDeletedCat.IsAvailable = true;
                    sDeletedCat.IconId = dto.IconId ?? sDeletedCat.IconId;
                    await _cCatRepository.UpdateCategoryAsync(sDeletedCat);
                    return Result.Success();
                }
                return Result.Failure("این دسته‌بندی از قبل وجود دارد.", ErrorCode.Duplicate);
            }

            var customCategory = new CustomFoodCategory
            {
                Name = name,
                IconId = dto.IconId ?? null,
                RestaurantId = restaurantId,
                IsAvailable = true,
                IsDeleted = false,
                GlobalCategoryId = dto.GlobalCategoryId ?? null
            };

            var created = await _cCatRepository.CreateAsync(customCategory);
            return created
                ? Result.Success()
                : Result.Failure("افزودن دسته‌بندی موفق نبود.", ErrorCode.Failure);
        }

        public async Task<Result> AddFromGlobalAsync(int globalCategoryId, int restaurantId)
        {
            var globalCat = await _gCatRepository.GetByIdAsync(globalCategoryId);
            if (globalCat == null)
                return Result.Failure("دسته‌بندی عمومی موردنظر یافت نشد.", ErrorCode.NotFound);

            var customCat = new CreateCustomFoodCategoryDto
            {
                Name = globalCat.Name,
                IconId = globalCat.IconId ?? null,
                GlobalCategoryId = globalCat.Id
            };

            return await AddCategoryAsync(customCat, restaurantId);
        }
        public async Task<List<GetCustomCategoryDto>> GetAllCustomFoodCategoriesAsync(int restaurantId)
        {
            var entities = await _cCatRepository.GetAllAsync(restaurantId);
            return entities.Select(category => new GetCustomCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                GlobalCategoryId = category.GlobalCategoryId,
                Icon = category.Icon == null ? null : new GetIconDto
                {
                    Id = category.Icon.Id,
                    FileName = category.Icon.FileName,
                    Label = category.Icon.Label,
                    Url = _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, category.Icon.FileName)
                }

            }).ToList();

        }
        public async Task<List<FoodCategorySelectListDto>> GetCustomFoodCategoriesAsync(int restaurantId)
        {
            var entities = await _cCatRepository.GetAllAsync(restaurantId);
            return entities.Select(c => new FoodCategorySelectListDto
            {
                Id = c.Id,
                Name = c.Name,
                GlobalCategoryId = c.GlobalCategoryId
            }).ToList();
        }
        public async Task<GetCustomCategoryDto> GetCategoryAsync(int catId)
        {
            var category = await _cCatRepository.GetByIdAsync(catId);
            var catDto = new GetCustomCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon == null ? null : new GetIconDto
                {
                    Id = category.Icon.Id,
                    FileName = category.Icon.FileName,
                    Label = category.Icon.Label,
                    Url = _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, category.Icon.FileName)
                }
            };
            return catDto;
        }
        public async Task<Result> UpdateCategoryAsync(UpdateCustomFoodCategoryDto dto)
        {
            var category = await _cCatRepository.GetByIdAsync(dto.Id);
            if (category == null)
                return Result.Failure("دسته‌بندی موردنظر یافت نشد.", ErrorCode.NotFound);

            var newName = (dto.Name ?? string.Empty).Trim();

            // فقط وقتی اسم واقعاً عوض شده چک تکراری بودن رو انجام بده
            // (وگرنه ذخیره‌ی بدون تغییر اسم هم duplicate حساب می‌شه)
            if (!string.Equals(category.Name, newName, StringComparison.Ordinal))
            {
                if (await _cCatRepository.ExistsByNameAsync(category.RestaurantId, newName))
                    return Result.Failure(
                        "این نام قبلاً برای دسته‌بندی دیگری استفاده شده است.",
                        ErrorCode.Duplicate);
            }

            category.Name = newName;
            category.IconId = dto.IconId;

            var updated = await _cCatRepository.UpdateCategoryAsync(category);
            return updated
                ? Result.Success()
                : Result.Failure("ذخیره تغییرات موفق نبود.", ErrorCode.Failure);
        }
        public async Task<bool> DeleteCustomCategoryAsync(int catId)
        {
            return await _cCatRepository.DeleteAsync(catId);
        }
    }
}

