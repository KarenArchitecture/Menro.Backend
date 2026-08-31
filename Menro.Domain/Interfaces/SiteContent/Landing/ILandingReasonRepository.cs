using Menro.Domain.Entities.SiteContent;

namespace Menro.Domain.Interfaces.SiteContent
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
