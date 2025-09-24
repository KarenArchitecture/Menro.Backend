using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    public class FoodCategoryService : IFoodCategoryService
    {
        private readonly IFoodCategoryRepository _repository;
        public FoodCategoryService(IFoodCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FoodCategoryDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(c => new FoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }
    }
}
