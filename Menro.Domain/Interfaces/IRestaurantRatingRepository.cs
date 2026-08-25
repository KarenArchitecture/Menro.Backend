using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantRatingRepository
    {
        Task<bool> UserCanRateRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default);
        Task<RestaurantRating?> GetByUserAndRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default);
        Task AddAsync(RestaurantRating rating, CancellationToken ct = default);
        Task<bool> SaveChangesAsync(CancellationToken ct = default);
        Task<(double Average, int Voters)> GetAggregateAsync(int restaurantId, CancellationToken ct = default);
    }
}