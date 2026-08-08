// Application/Features/Restaurants/Services/Interfaces/IRestaurantPaymentSettingsService.cs
using Menro.Application.Features.Restaurants.DTOs;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantPaymentSettingsService
    {
        Task<RestaurantPaymentMethodDto> GetAsync(int restaurantId);
        Task SetAsync(int restaurantId, UpdateRestaurantPaymentMethodDto dto);
    }
}