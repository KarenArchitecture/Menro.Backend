using Menro.Application.Features.Restaurants.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId);
        Task<List<RestaurantCategoryDto>> GetRestaurantCategoriesAsync();
        Task<string> GenerateUniqueSlugAsync(string name);
        Task<bool> IsSlugAvailableAsync(string slug, int excludeRestaurantId);
        Task<Restaurant?> GetRestaurantByIdAsync(int id);
        Task<int> GetRestaurantIdByUserIdAsync(string userId);
        Task<string> GetRestaurantName(int restaurantId);



        // owner methods
        Task<RestaurantProfileDto?> GetRestaurantProfileAsync(int id);
        Task UpdateRestaurantProfileAsync(UpdateRestaurantProfileDto dto);


        // admin panel => restaurant category management tab
        Task<RestaurantCategoryDto?> GetRestaurantCategoryByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateRestaurantCategoryAsync(CreateRestaurantCategoryDto dto);
        Task<(bool Success, string? Error)> UpdateRestaurantCategoryAsync(UpdateRestaurantCategoryDto dto);
        Task<(bool Success, string? Error)> DeleteRestaurantCategoryAsync(int id);

    }
}
