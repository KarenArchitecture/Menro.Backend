// Domain/Interfaces/IFoodRatingRepository.cs
using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IFoodRatingRepository : IRepository<FoodRating>
    {
        Task<FoodRating?> GetByFoodAndUserAsync(int foodId, string userId);
        Task AddAsync(FoodRating rating);
        Task SaveChangesAsync();
    }
}