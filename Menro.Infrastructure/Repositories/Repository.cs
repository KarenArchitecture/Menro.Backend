using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MenroDbContext _context;
        internal DbSet<T> Set;

        public Repository(MenroDbContext context)
        {
            _context = context;
            Set = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperty = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Set : Set.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperty))
            {
                foreach (var include in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperty = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Set : Set.AsNoTracking();

            query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperty))
            {
                foreach (var include in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await Set.FindAsync(id);
        }

        public async Task<bool> AddAsync(T entity)
        {
            try
            {
                await Set.AddAsync(entity);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                Set.Update(entity);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                Set.Remove(entity);
                // Remove synchronous چون async نیست
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await Set.AnyAsync(filter);
        }
    }
}
