using Menro.Application.Features.Ads.DTOs;

namespace Menro.Application.Features.Ads.Services
{
    public interface IAdSettingsService
    {
        Task<List<AdPricingSettingDto>> GetActiveSettingsAsync(AdPlacementType placement);
        Task<bool> SaveSettingsAsync(List<AdPricingSettingDto> dtos);
    }
}
