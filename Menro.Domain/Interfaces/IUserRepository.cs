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

        /* ============================================================
           ADMIN's User management
           Both search methods return raw User entities with Restaurants/
           Orders/FavoriteFoods eagerly loaded (Include). Counting them,
           mapping to DTOs, and any other shaping is the service's job.
        ============================================================ */

        /// <summary>
        /// Searches users by free-text (FullName/Email/PhoneNumber/UserName, OR),
        /// optional role name, and optional suspension status, paged.
        /// </summary>
        Task<(List<User> Items, int TotalCount)> SearchUsersAsync(
            string? search, string? role, int page, int pageSize);

        /// <summary>
        /// Retrieves a single user with Restaurants/Orders/FavoriteFoods loaded.
        /// </summary>
        Task<User?> GetByIdWithDetailsAsync(string id);

        /// <summary>
        /// Bulk-loads role names for a set of user ids in a single query.
        /// </summary>
        Task<Dictionary<string, List<string>>> GetRolesForUserIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Every role name that exists in the system.
        /// </summary>
        Task<List<string>> GetAllRoleNamesAsync();
    }
}
