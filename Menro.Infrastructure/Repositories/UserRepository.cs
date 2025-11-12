using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing User entities.
    /// Handles lookups and existence checks by email, phone, or name.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly MenroDbContext _context;

        public UserRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        /* ============================================================
           🔹 Lookup Methods
        ============================================================ */

        /// <summary>
        /// Checks whether a user exists with the specified email.
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Retrieves a user by phone number.
        /// </summary>
        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        /// <summary>
        /// Retrieves a user by full name.
        /// </summary>
        public async Task<User?> GetByNameAsync(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.FullName == name);
        }
    }
}
