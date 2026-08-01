using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantTableRepository : IRepository<RestaurantTable>
    {
        Task<List<RestaurantTable>> GetAllByRestaurantIdAsync(int restaurantId);
        Task<RestaurantTable?> GetByIdAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}