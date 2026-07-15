using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Domain.Entities.Landing;

namespace Menro.Domain.Interfaces.Landing
{
    public interface ILandingReasonRepository
    {
        Task<List<LandingReason>> GetAllOrderedAsync();

        Task<LandingReason?> GetByIdAsync(Guid id);

        /// <summary>Next free SortOrder value (current max + 1, or 0 if empty).</summary>
        Task<int> GetNextSortOrderAsync();

        Task AddAsync(LandingReason entity);

        Task UpdateAsync(LandingReason entity);

        /// <summary>Persists SortOrder changes for two swapped rows in one call.</summary>
        Task UpdateRangeAsync(LandingReason first, LandingReason second);

        Task DeleteAsync(LandingReason entity);
    }
}
