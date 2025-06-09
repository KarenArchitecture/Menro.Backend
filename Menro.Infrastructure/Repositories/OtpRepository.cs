using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class OtpRepository : Repository<Otp>, IOtpRepository
    {
        private readonly MenroDbContext _context;

        public OtpRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Otp?> GetLatestUnexpiredAsync(string phoneNumber)
        {
            // انتقال به سرویس و استفاده از generic repository
            return await _context.Set<Otp>()
                .Where(o => o.PhoneNumber == phoneNumber &&
                            !o.IsUsed &&
                            o.ExpirationTime > DateTime.UtcNow)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();
        }
    }
}
