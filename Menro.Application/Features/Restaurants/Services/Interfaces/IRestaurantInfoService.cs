using Menro.Application.Features.Restaurants.DTOs;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantInfoService
    {
        Task<RestaurantInfoDto?> GetInfoBySlugAsync(string slug, CancellationToken ct = default);
    }
}