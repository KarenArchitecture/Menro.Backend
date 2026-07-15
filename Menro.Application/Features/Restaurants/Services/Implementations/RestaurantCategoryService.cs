using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
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
