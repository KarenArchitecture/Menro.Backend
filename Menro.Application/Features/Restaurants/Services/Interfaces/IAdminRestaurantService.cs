using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Common.Models;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IAdminRestaurantService
    {
        Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(RestaurantStatus status);
        Task<RestaurantDetailsForAdminDto?> GetRestaurantDetailsForAdminAsync(int id);
        Task<PagedResult<RestaurantOverviewDto>> GetRestaurantsOverviewAsync(string? search, int? categoryId, int page, int pageSize);
        Task<bool> ApproveRestaurantAsync(int restaurantId, bool approve);
        Task<bool> UpdateRestaurantStatusAsync(int restaurantId, RestaurantStatus status, string? rejectReason);
    }
}
