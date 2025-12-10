using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation providing basic CRUD operations.
    /// Acts as a shared foundation for all specific repositories.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MenroDbContext _context;
        internal DbSet<T> Set;

        public Repository(MenroDbContext context)
        {
            _context = context;
            Set = _context.Set<T>();
        }

        /* ============================================================
           🔹 Basic Query Methods
        ============================================================ */

        /// <summary>
        /// Retrieves all entities, optionally filtered and including related properties.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperty = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Set : Set.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperty))
            {
                foreach (var include in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(include.Trim());
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Retrieves a single entity by a filter, with optional includes.
        /// </summary>
        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperty = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Set : Set.AsNoTracking();
            query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperty))
            {
                foreach (var include in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(include.Trim());
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds an entity by its primary key.
        /// </summary>
        public async Task<T?> GetByIdAsync(string id)
        {
            return await Set.FindAsync(id);
        }

        /* ============================================================
           ⚙️ CRUD Operations
        ============================================================ */

        /// <summary>
        /// Adds an entity to the context.
        /// </summary>
        public async Task<bool> AddAsync(T entity)
        {
            try
            {
                await Set.AddAsync(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                Set.Update(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes an entity from the context.
        /// </summary>
        public async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                Set.Remove(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether any entity satisfies a given condition.
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await Set.AnyAsync(filter);
        }
    }
}
