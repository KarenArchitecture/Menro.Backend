using Menro.Application.Restaurants.DTOs;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantBannerService
    {
        Task<RestaurantBannerDto?> GetBannerBySlugAsync(string slug);
        void InvalidateCache(string slug);
    }
}
