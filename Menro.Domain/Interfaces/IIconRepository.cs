using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IIconRepository
    {
        Task<List<Icon>> GetAllAsync();
        Task<Icon?> GetByIdAsync(int id);
        Task<bool> AddAsync(Icon icon);
        Task<bool> DeleteAsync(int id);
    }
}
