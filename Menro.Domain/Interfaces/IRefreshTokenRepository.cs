using Menro.Domain.Entities.Identity;

namespace Menro.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddAsync(RefreshToken entity);
        Task<RefreshToken?> FindByHashAsync(string tokenHash);
        Task<bool> UpdateAsync(RefreshToken entity);
        Task<List<RefreshToken>> GetActiveByUserAsync(string userId);
    }
}
