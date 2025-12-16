using Menro.Domain.Enums;

namespace Menro.Domain.Interfaces
{
    public interface IAdPricingSettingRepository
    {
        Task<List<AdPricingSetting>> GetActiveSettingsAsync(AdPlacementType placementType);
        Task SaveSettingsAsync(List<AdPricingSetting> settings);
    }
}
