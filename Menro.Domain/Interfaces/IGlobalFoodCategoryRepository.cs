using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IGlobalFoodCategoryRepository
    {
        Task<List<GlobalFoodCategory>> GetAllAsync();
        Task<GlobalFoodCategory> GetByIdAsync(int id);
        Task<bool> CreateAsync(GlobalFoodCategory category);


    }
}
