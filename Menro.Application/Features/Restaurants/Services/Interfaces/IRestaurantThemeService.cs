using Menro.Application.Common.Models;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantThemeService
    {
        Task<string?> GetThemeIdAsync(string userId);
        Task<Result> SaveThemeAsync(string userId, string themeId);
    }
}