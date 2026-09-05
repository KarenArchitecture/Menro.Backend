using Menro.Domain.Entities.SiteContent;

namespace Menro.Domain.Interfaces.SiteContent
{
    public interface ILandingFaqRepository
    {
        Task<List<LandingFaq>> GetAllOrderedAsync();

        Task<LandingFaq?> GetByIdAsync(Guid id);

        /// <summary>Next free SortOrder value (current max + 1, or 0 if empty).</summary>
        Task<int> GetNextSortOrderAsync();

        Task AddAsync(LandingFaq entity);

        Task UpdateAsync(LandingFaq entity);

        /// <summary>Persists SortOrder changes for two swapped rows in one call.</summary>
        Task UpdateRangeAsync(LandingFaq first, LandingFaq second);

        Task DeleteAsync(LandingFaq entity);
    }
}
