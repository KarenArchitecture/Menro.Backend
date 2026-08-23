using Menro.Application.Features.Restaurants.DTOs;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRestaurantRatingService
    {
        Task<RestaurantRatingResultDto> SubmitRatingAsync(string userId, RateRestaurantRequestDto dto, CancellationToken ct = default);
        Task<RestaurantRatingResultDto> GetRatingSummaryAsync(string? userId, int restaurantId, CancellationToken ct = default);
    }
}