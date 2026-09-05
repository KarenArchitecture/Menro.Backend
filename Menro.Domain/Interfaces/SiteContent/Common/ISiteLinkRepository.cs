using Menro.Domain.Entities.SiteContent;

namespace Menro.Domain.Interfaces.SiteContent
{
    public interface ISiteLinkRepository
    {
        Task<List<SiteLink>> GetByLocationAsync(MenuLocation location, bool includeInactive = false);
        Task<SiteLink?> GetByIdAsync(Guid id);
        Task<List<SiteLink>> GetAllAsync();
        Task<int> GetMaxOrderAsync(MenuLocation location);
        Task AddAsync(SiteLink entity);
        Task UpdateAsync(SiteLink entity);
        Task RemoveAsync(SiteLink entity);
        Task ReorderAsync(List<SiteLink> items);
    }
}