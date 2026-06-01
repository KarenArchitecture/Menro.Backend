using Menro.Application.DTO;
using Menro.Application.Restaurants.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Enums;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId);
        Task<List<RestaurantCategoryDto>> GetRestaurantCategoriesAsync();
        Task<string> GenerateUniqueSlugAsync(string name);
        Task<Restaurant?> GetRestaurantByIdAsync(int id);
        Task<int> GetRestaurantIdByUserIdAsync(string userId);
        Task<string> GetRestaurantName(int restaurantId);


        // admin panel => restaurant management tab
        Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(RestaurantStatus status);
        Task<RestaurantDetailsForAdminDto?> GetRestaurantDetailsForAdminAsync(int id);

        Task<bool> ApproveRestaurantAsync(int restaurantId, bool approve);
        Task<bool> UpdateRestaurantStatusAsync(int restaurantId, RestaurantStatus status, string? rejectReason);
        Task<RestaurantProfileDto?> GetRestaurantProfileAsync(int id);
        Task UpdateRestaurantProfileAsync(UpdateRestaurantProfileDto dto);

    }
}
