using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.Features.GlobalFoodCategories.DTOs;


namespace Menro.Application.Features.GlobalFoodCategories.Services.Implementations
{
    public class GlobalFoodCategoryService : IGlobalFoodCategoryService
    {
        private readonly IGlobalFoodCategoryRepository _repository;
        public GlobalFoodCategoryService(IGlobalFoodCategoryRepository repository)
        {
            _repository = repository;
        }

        // modify for icon url/name
        public async Task<bool> CreateGlobalCategoryAsync(CreateGlobalCategoryDTO dto)
        {
            var entity = new GlobalFoodCategory
            {
                Name = dto.Name,
                IsActive = true
            };

            if (await _repository.CreateAsync(entity))
            {
                return true;
            }

            return false;

        }
        public async Task<List<GlobalCategoryDTO>> GetAllGlobalCategoriesAsync()
        {
            var list = await _repository.GetAllAsync();

            return list.Select(x => new GlobalCategoryDTO
            {
                Id = x.Id,
                Name = x.Name,
                SvgIcon = x.SvgIcon
            }).ToList();
        }
        public async Task<GlobalCategoryDTO> GetGlobalCategoryAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            return new GlobalCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                SvgIcon = category.SvgIcon
            };
        }


    }
}
