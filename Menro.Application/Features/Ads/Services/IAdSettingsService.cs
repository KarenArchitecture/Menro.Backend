using Menro.Application.Features.Ads.DTOs;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Ads.Services
{
    public interface IAdSettingsService
    {
        Task<List<AdPricingSettingDto>> GetActiveSettingsAsync(AdPlacementType placement);
        Task SaveSettingsAsync(List<AdPricingSettingDto> dtos);
    }
}
