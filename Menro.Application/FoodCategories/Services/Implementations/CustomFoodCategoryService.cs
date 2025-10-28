using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Application.Features.CustomFoodCategory.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Icons.DTOs;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    public class CustomFoodCategoryService : ICustomFoodCategoryService
    {
        #region DI
        private readonly ICustomFoodCategoryRepository _cCatRepository;
        private readonly IGlobalFoodCategoryRepository _gCatRepository;
        private readonly IFileUrlService _fileUrlService;
        private readonly ICurrentUserService _currentUserService;
        public CustomFoodCategoryService(
            ICustomFoodCategoryRepository cCatRepository,
            IGlobalFoodCategoryRepository gCatRepository,
            ICurrentUserService currentUserService,
            IFileUrlService fileUrlService
            )
        {
            _cCatRepository = cCatRepository;
            _gCatRepository = gCatRepository;
            _currentUserService = currentUserService;
            _fileUrlService = fileUrlService;
        }
        #endregion
        public async Task<bool> AddCategoryAsync (CreateCustomFoodCategoryDto dto)
        {
            if (dto is null) return false; // invalid model

            var name = (dto.Name ?? string.Empty).Trim();

            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == null || restaurantId == 0) return false; // invalid restaurantId

            if (await _cCatRepository.ExistsByNameAsync(restaurantId, name))
            {
                if (await _cCatRepository.IsSoftDeleted(restaurantId, name))
                {
                    var sDeletedCat = await _cCatRepository.GetByNameAsync(restaurantId, name);
                    sDeletedCat.IsDeleted = false;
                    sDeletedCat.IsAvailable = true;
                    sDeletedCat.IconId = dto.IconId ?? sDeletedCat.IconId;
                    await _cCatRepository.UpdateAsync(sDeletedCat);
                    return true;
                }
                return false; // duplicate
            }
            
            var customCategory = new CustomFoodCategory
            {
                Name = name,
                IconId = dto.IconId ?? null,
                RestaurantId = restaurantId,
                IsAvailable = true,
                IsDeleted = false,
                GlobalCategoryId = null
            };
            return await _cCatRepository.CreateAsync(customCategory);
        }
        public async Task<bool> AddFromGlobalAsync(int globalCategoryId, int restaurantId)
        {
            var globalCat = await _gCatRepository.GetByIdAsync(globalCategoryId);
            if (globalCat == null)
                throw new Exception("Global category not found");
            var customCat = new CustomFoodCategory
            {
                Name = globalCat.Name,
                IconId = globalCat.IconId,
                RestaurantId = restaurantId,
                IsAvailable = true,
                IsDeleted = false,

                GlobalCategoryId = globalCategoryId,
                GlobalCategory = globalCat,
            };
            

            return await _cCatRepository.CreateAsync(customCat);
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
                    Url = _fileUrlService.BuildIconUrl(category.Icon.FileName)
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
                    Url = _fileUrlService.BuildIconUrl(category.Icon.FileName)
                }

            };
            return catDto;
        }
        public async Task<bool> UpdateCategoryAsync(UpdateCustomFoodCategoryDto dto)
        {
            // دسته رو از دیتابیس بیار
            var category = await _cCatRepository.GetByIdAsync(dto.Id); // فرض بر اینه که متد GetByIdAsync موجوده

            if (category == null || category.IsDeleted)
                return false; // دسته وجود ندارد یا soft deleted است

            // آپدیت فیلدها
            category.Name = dto.Name;
            category.IconId = dto.IconId;

            return await _cCatRepository.UpdateCategoryAsync(category);
        }
        public async Task<bool> DeleteCustomCategoryAsync(int catId)
        {
            bool result = await _cCatRepository.DeleteAsync(catId);
            return result;
        }
    }
}

