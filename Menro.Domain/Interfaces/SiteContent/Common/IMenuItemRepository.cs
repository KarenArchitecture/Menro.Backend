using Menro.Domain.Entities.SiteContent;

namespace Menro.Domain.Interfaces.SiteContent
{
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> GetByLocationAsync(MenuLocation location, bool includeInactive = false);
        Task<MenuItem?> GetByIdAsync(Guid id);
        Task<List<MenuItem>> GetAllAsync();
        Task<int> GetMaxOrderAsync(MenuLocation location);
        Task AddAsync(MenuItem entity);
        Task UpdateAsync(MenuItem entity);
        Task RemoveAsync(MenuItem entity);
        Task ReorderAsync(List<MenuItem> items);
    }
}