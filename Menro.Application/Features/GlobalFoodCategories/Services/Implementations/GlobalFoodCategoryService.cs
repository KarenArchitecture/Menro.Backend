using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.GlobalFoodCategories.Services.Implementations
{
    public class GlobalFoodCategoryService : IGlobalFoodCategoryService
    {
        private readonly IGlobalFoodCategoryRepository _repository;
        public GlobalFoodCategoryService(IGlobalFoodCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> CreateCategoryAsync(CreateGlobalCategoryDTO dto)
        {
            var entity = new Domain.Entities.GlobalFoodCategory
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

        public async Task<List<GlobalCategoryDTO>> GetAllCategoriesAsync()
        {
            var list = await _repository.GetAllAsync();

            return list.Select(x => new GlobalCategoryDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }
        public async Task<GlobalCategoryDTO> GetGlobalCategoryAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            return new GlobalCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
            };
        }


    }
}
