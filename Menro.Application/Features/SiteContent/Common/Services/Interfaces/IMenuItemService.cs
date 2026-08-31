using Menro.Application.Features.SiteContent.DTOs;
using Menro.Domain.Entities.SiteContent;

namespace Menro.Application.Features.SiteContent.Services.Interfaces
{
    public interface IMenuItemService
    {
        /// <summary>برای فرانت عمومی سایت - فقط آیتم‌های فعال.</summary>
        Task<List<MenuItemDto>> GetPublicMenuAsync(MenuLocation location);

        /// <summary>برای پنل ادمین - همه‌ی آیتم‌ها (فعال و غیرفعال).</summary>
        Task<List<MenuItemDto>> GetAdminMenuAsync(MenuLocation location);

        Task<List<MenuItemDto>> GetAllAsync();

        Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto);
        Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto dto);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(MenuLocation location, ReorderMenuItemDto dto);
    }
}
