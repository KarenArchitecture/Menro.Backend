using Menro.Domain.Interfaces;
using Menro.Domain.Entities.Identity;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MenroDbContext _context;

        public RefreshTokenRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(RefreshToken entity)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<RefreshToken?> FindByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        }

        public async Task<bool> UpdateAsync(RefreshToken entity)
        {
            try
            {
                _context.RefreshTokens.Update(entity);
                await Task.CompletedTask;
                return true;    
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<RefreshToken>> GetActiveByUserAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
