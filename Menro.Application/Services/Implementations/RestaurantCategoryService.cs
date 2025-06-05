using Menro.Application.DTO;
using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Implementations
{
    public class RestaurantCategoryService : IRestaurantCategoryService
    {
        private readonly IUnitOfWork _uow;
        public RestaurantCategoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<RestaurantCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _uow.RestaurantCategory.GetAllAsync();

            return categories
                .Select(c => new RestaurantCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();
        }
    }
}
