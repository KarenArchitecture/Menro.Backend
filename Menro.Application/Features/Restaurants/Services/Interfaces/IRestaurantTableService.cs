using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.DTOs.RestaurantTables;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantTableService
    {
        Task<List<RestaurantTablesDto>> GetAllByRestaurantIdAsync(int restaurantId);
        Task<List<RestaurantTableListItemDto>> GetAllByRestaurantIdForPublicAsync(int restaurantId);
        Task<Result> AddTableAsync(CreateRestaurantTableDto dto, int restaurantId);
        Task<Result> UpdateTableAsync(UpdateRestaurantTableDto dto);
        Task<bool> DeleteTableAsync(int tableId);
    }
}