using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface IOtpRepository : IRepository<Otp>
    {
        Task<Otp?> GetLatestUnexpiredAsync(string phoneNumber);
    }
}
