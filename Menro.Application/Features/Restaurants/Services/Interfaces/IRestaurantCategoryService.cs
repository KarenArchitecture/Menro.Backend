using Menro.Application.Features.Restaurants.DTOs;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantCategoryService
    {
        Task<List<RestaurantCategoryDto>> GetAllCategoriesAsync();
    }
}
