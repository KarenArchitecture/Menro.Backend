using Menro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantRatingRepository : IRepository<RestaurantRating>
    {
        Task<RestaurantRating?> GetByUserAndRestaurantAsync(string userId, int restaurantId);

        Task<Dictionary<int, int>> GetUserRatingsForRestaurantsAsync(string userId, List<int> restaurantIds);

        Task SaveChangesAsync();
    }
}