using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing User entities.
    /// Handles lookups and existence checks by email, phone, or name,
    /// plus admin-panel search/pagination/role queries.
    ///
    /// Returns raw User entities only (with related collections eagerly
    /// loaded where needed) — counting, mapping, and any shaping happens
    /// in UserManagementService, not here.
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

        /* ============================================================
           🔹 Admin Panel: Search / Pagination / Roles
        ============================================================ */

        /// <summary>
        /// Searches users by free-text (FullName/Email/PhoneNumber/UserName, OR),
        /// optional role name, and optional suspension status, paged.
        /// Restaurants/Orders/FavoriteFoods are eagerly loaded so the service
        /// can count them without an extra roundtrip per user.
        /// </summary>
        public async Task<(List<User> Items, int TotalCount)> SearchUsersAsync(
            string? search, string? role, int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(term) ||
                    (u.Email != null && u.Email.Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)) ||
                    (u.UserName != null && u.UserName.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var userIdsInRole = _context.UserRoles
                    .Where(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role))
                    .Select(ur => ur.UserId);

                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }        /// <summary>
                 /// Retrieves a single user with Restaurants/Orders/FavoriteFoods loaded,
                 /// for refreshing the "مشاهده اطلاعات کاربر" modal.
                 /// </summary>
        public async Task<User?> GetByIdWithDetailsAsync(string id)
        {
            return await _context.Users
                .Include(u => u.Restaurants)
                .Include(u => u.Orders)
                .Include(u => u.FavoriteFoods)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Bulk-loads role names for a set of user ids in a single query
        /// (avoids one roundtrip per row when building the list page).
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetRolesForUserIdsAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.ToList();
            if (ids.Count == 0)
                return new Dictionary<string, List<string>>();

            var rolePairs = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where ids.Contains(ur.UserId)
                select new { ur.UserId, RoleName = r.Name }
            ).ToListAsync();

            return rolePairs
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName ?? string.Empty).ToList());
        }

        /// <summary>
        /// Every role name in the system, for the filter dropdown and the
        /// "ویرایش نقش‌ها" modal checkbox list.
        /// </summary>
        public async Task<List<string>> GetAllRoleNamesAsync()
        {
            return await _context.Roles
                .Where(r => r.Name != null)
                .Select(r => r.Name!)
                .OrderBy(name => name)
                .ToListAsync();
        }
    }
}
