using Menro.Domain.Entities;
using System.Linq.Expressions;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantCategoryRepository
    {
        Task<List<RestaurantCategory>> GetAllAsync();
        Task<RestaurantCategory?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<RestaurantCategory, bool>> predicate);
        Task AddAsync(RestaurantCategory category);
        Task DeleteAsync(RestaurantCategory category);

        Task<bool> IsNameTakenAsync(string name, int? excludeId = null);
        Task<bool> SaveChangesAsync();
    }
}
