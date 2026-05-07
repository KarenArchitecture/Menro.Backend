using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IMusicRepository
    {
        Task AddAsync(Music music);
        Task UpdateAsync(Music music);
        Task DeleteAsync(Music music);

        Task<Music?> GetByIdAsync(Guid id);
        Task<List<Music>> GetAllAsync();
        Task<List<Music>> SearchAsync(string searchTerm);
    }
}
