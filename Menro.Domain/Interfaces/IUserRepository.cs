using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing User entities,
    /// including lookup and validation helpers.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /* ============================================================
           🔹 Retrieval Methods
        ============================================================ */

        /// <summary>
        /// Retrieves a user by email address.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by phone number.
        /// </summary>
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Retrieves a user by full name.
        /// </summary>
        Task<User?> GetByNameAsync(string name);

        /* ============================================================
           🔎 Validation
        ============================================================ */

        /// <summary>
        /// Checks whether a user exists with the specified email.
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);
    }
}
