using Menro.Application.Features.Identity.Services;
using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    public class CustomFoodCategoryService : ICustomFoodCategoryService
    {
        private readonly ICustomFoodCategoryRepository _cCatRepository;
        private readonly IGlobalFoodCategoryRepository _gCatRepository;
        //private readonly ICurrentUserService _currentUserService;
        public CustomFoodCategoryService(ICustomFoodCategoryRepository cCatRepository, IGlobalFoodCategoryRepository gCatRepository, ICurrentUserService currentUserService)
        {
            _cCatRepository = cCatRepository;
            _gCatRepository = gCatRepository;
            //_currentUserService = currentUserService;
        }

        public async Task<bool> AddFromGlobalAsync(int globalCategoryId, int restaurantId)
        {
            var globalCat = await _gCatRepository.GetByIdAsync(globalCategoryId);
            if (globalCat == null)
                throw new Exception("Global category not found");
            var customCat = new CustomFoodCategory
            {
                Name = globalCat.Name,
                SvgIcon = globalCat.SvgIcon,
                RestaurantId = restaurantId,
                IsAvailable = true,
                IsDeleted = false,

                GlobalCategoryId = globalCategoryId,
                GlobalCategory = globalCat,
            };
            

            return await _cCatRepository.CreateAsync(customCat);
        }


        public async Task<List<FoodCategoryDto>> GetCustomFoodCategoriesAsync(int restaurantId)
        {
            var entities = await _cCatRepository.GetCustomFoodCategoriesAsync(restaurantId);
            return entities.Select(c => new FoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }


    }
}

//public Task<FoodDetailsDto> CreateFoodCategoryAsync(CreateFoodDto dto, int restaurantId)
//{
//    throw new NotImplementedException();
//}
//public Task<FoodCategoryDto> GetByIdAsync(int foodCategoryId)
//{
//    throw new NotImplementedException();
//}

//public Task<List<FoodsListItemDto>> GetFoodCategoriesListAsync(int restaurantId)
//{
//    throw new NotImplementedException();
//}

//public Task<FoodDetailsDto?> GetFoodCategoryAsync(int foodCategoryId, int restaurantId)
//{
//    throw new NotImplementedException();
//}
//public Task<bool> DeleteFoodCategoryAsync(int foodCategoryId)
//{
//    throw new NotImplementedException();
//}
