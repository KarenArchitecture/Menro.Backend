using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Features.GlobalFoodCategories.Services.Interfaces;
using Menro.Application.Features.GlobalFoodCategories.DTOs;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;


namespace Menro.Application.Features.GlobalFoodCategories.Services.Implementations
{
    public class GlobalFoodCategoryService : IGlobalFoodCategoryService
    {
        private readonly IGlobalFoodCategoryRepository _repository;
        private readonly IFileUrlService _fileUrlService;
        public GlobalFoodCategoryService(IGlobalFoodCategoryRepository repository, IFileUrlService fileUrlService)
        {
            _repository = repository;
            _fileUrlService = fileUrlService;
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
                Icon = x.Icon == null ? null : new GetIconDto
                {
                    Id = x.Icon.Id,
                    FileName = x.Icon.FileName,
                    Label = x.Icon.Label,
                    Url = _fileUrlService.BuildIconUrl(x.Icon.FileName)
                }
            }).ToList();
        }
        public async Task<GlobalCategoryDTO> GetGlobalCategoryAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            return new GlobalCategoryDTO
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
        }


    }
}
