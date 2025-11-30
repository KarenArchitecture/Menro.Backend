using Menro.Application.DTO;
using Menro.Application.Restaurants.DTOs;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId);
        Task<List<RestaurantCategoryDto>> GetRestaurantCategoriesAsync();
        Task<string> GenerateUniqueSlugAsync(string name);
        Task<int> GetRestaurantIdByUserIdAsync(string userId);
        Task<string> GetRestaurantName(int restaurantId);


        // admin panel => restaurant management tab
        Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(bool? approved);
        Task<RestaurantDetailsForAdminDto?> GetRestaurantDetailsForAdminAsync(int id);

        Task<bool> ApproveRestaurantAsync(int restaurantId, bool approve);
    }
}
