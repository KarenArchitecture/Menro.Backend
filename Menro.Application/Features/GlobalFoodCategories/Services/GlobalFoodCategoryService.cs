using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.Models;


namespace Menro.Application.Features.GlobalFoodCategories.Services
{
    public class GlobalFoodCategoryService : IGlobalFoodCategoryService
    {
        private readonly IGlobalFoodCategoryRepository _repository;
        private readonly IMediaStorageProvider _mediaStorage;
        public GlobalFoodCategoryService(IGlobalFoodCategoryRepository repository, IMediaStorageProvider mediaStorage)
        {
            _repository = repository;
            _mediaStorage = mediaStorage;
        }

        // modify for icon url/name
        public async Task<Result> AddGlobalCategoryAsync(CreateGlobalCategoryDTO dto)
        {
            var name = (dto.Name ?? string.Empty).Trim();

            if (await _repository.ExistsByNameAsync(name))
                return Result.Failure("این دسته‌بندی از قبل وجود دارد.", ErrorCode.Duplicate);

            var entity = new GlobalFoodCategory
            {
                Name = name,
                IconId = dto.IconId,
                IsActive = true
            };

            var created = await _repository.CreateAsync(entity);
            return created
                ? Result.Success()
                : Result.Failure("افزودن دسته‌بندی موفق نبود.", ErrorCode.Failure);
        }
        public async Task<List<GetGlobalCategoryDTO>> GetAllGlobalCategoriesAsync()
        {
            var list = await _repository.GetAllAsync();

            return list.Select(x => new GetGlobalCategoryDTO
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon == null ? null : new GetIconDto
                {
                    Id = x.Icon.Id,
                    FileName = x.Icon.FileName,
                    Label = x.Icon.Label,
                    Url = _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, x.Icon.FileName)
                }
            }).ToList();
        }
        public async Task<GetGlobalCategoryDTO> GetGlobalCategoryAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            return new GetGlobalCategoryDTO
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
        }
        public async Task<Result> UpdateGlobalCategoryAsync(UpdateGlobalCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(dto.Id);
            if (category == null)
                return Result.Failure("دسته‌بندی موردنظر یافت نشد.", ErrorCode.NotFound);

            var newName = (dto.Name ?? string.Empty).Trim();

            // فقط وقتی اسم واقعاً عوض شده چک تکراری بودن رو انجام بده
            if (!string.Equals(category.Name, newName, StringComparison.Ordinal))
            {
                if (await _repository.ExistsByNameAsync(newName))
                    return Result.Failure(
                        "این نام قبلاً برای دسته‌بندی دیگری استفاده شده است.",
                        ErrorCode.Duplicate);
            }

            category.Name = newName;
            category.IconId = dto.IconId;

            var updated = await _repository.UpdateCategoryAsync(category);
            return updated
                ? Result.Success()
                : Result.Failure("ذخیره تغییرات موفق نبود.", ErrorCode.Failure);
        }
        public async Task<bool> DeleteGlobalCategoryAsync(int id)
        {
            return await _repository.DeleteCategoryAsync(id);
        }
    }
}
