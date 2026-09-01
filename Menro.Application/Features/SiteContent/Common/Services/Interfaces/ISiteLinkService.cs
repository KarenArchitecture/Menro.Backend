using Menro.Application.Features.SiteContent.DTOs;
using Menro.Domain.Entities.SiteContent;

namespace Menro.Application.Features.SiteContent.Services.Interfaces
{
    public interface ISiteLinkService
    {
        /// <summary>برای فرانت عمومی سایت - فقط آیتم‌های فعال.</summary>
        Task<List<SiteLinkDto>> GetPublicMenuAsync(MenuLocation location);

        /// <summary>برای پنل ادمین - همه‌ی آیتم‌ها (فعال و غیرفعال).</summary>
        Task<List<SiteLinkDto>> GetAdminMenuAsync(MenuLocation location);

        Task<List<SiteLinkDto>> GetAllAsync();

        Task<SiteLinkDto> CreateAsync(CreateSiteLinkDto dto);
        Task<SiteLinkDto> UpdateAsync(Guid id, UpdateSiteLinkDto dto);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(MenuLocation location, ReorderSiteLinkDto dto);
    }
}
