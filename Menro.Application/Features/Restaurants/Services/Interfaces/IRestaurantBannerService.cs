using Menro.Application.Features.Restaurants.DTOs;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantBannerService
    {
        Task<RestaurantBannerDto?> GetBannerBySlugAsync(string slug);
        void InvalidateCache(string slug);
    }
}
