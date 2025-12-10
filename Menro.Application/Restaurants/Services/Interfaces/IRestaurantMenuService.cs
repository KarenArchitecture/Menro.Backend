using Menro.Application.Restaurants.DTOs;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    /// <summary>
    /// Service for building restaurant menu structure (categories + foods).
    /// Used by the public restaurant page.
    /// </summary>
    public interface IRestaurantMenuService
    {
        /// <summary>
        /// Returns full restaurant menu grouped by categories using restaurant slug.
        /// Lightweight version (no variants/addons).
        /// </summary>
        Task<List<RestaurantMenuDto>> GetMenuBySlugAsync(string slug);
    }
}
