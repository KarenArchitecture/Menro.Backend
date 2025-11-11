using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Generic repository contract providing shared CRUD and query operations.
    /// Used as the base for all domain repositories.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /* ============================================================
           🔹 Query Methods
        ============================================================ */

        /// <summary>
        /// Returns all entities matching an optional filter and includes navigation properties if specified.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperty = null, bool tracked = false);

        /// <summary>
        /// Returns a single entity that matches a given filter (with optional includes).
        /// </summary>
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperty = null, bool tracked = false);

        /// <summary>
        /// Finds an entity by its primary key (string-based ID).
        /// </summary>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Checks if any record matches a given condition.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

        /* ============================================================
           ⚙️ CRUD Operations
        ============================================================ */

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task<bool> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        Task<bool> DeleteAsync(T entity);
    }
}
