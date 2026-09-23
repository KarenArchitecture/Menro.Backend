using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using System.Threading.Tasks;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantRatingService
    {
        Task<Result> RateRestaurantAsync(string userId, RateRestaurantDto dto);
        Task<int?> GetUserRatingAsync(string userId, int restaurantId);
    }
}