using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;

namespace Menro.Application.Features.Ads.Services
{
     class AdSettingsService : IAdSettingsService
    {
        private readonly IAdPricingSettingRepository _repository;
        public AdSettingsService(IAdPricingSettingRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<AdPricingSettingDto>> GetActiveSettingsAsync(AdPlacementType placement)
        {
            var list = await _repository.GetActiveSettingsAsync(placement);

            var dto = list.Select(x => new AdPricingSettingDto
            {
                Id = x.Id,
                PlacementType = x.PlacementType,
                BillingType = x.BillingType,

                MinUnits = x.MinUnits,
                MaxUnits = x.MaxUnits,
                UnitPrice = x.UnitPrice,

                IsActive = x.IsActive
            }).ToList();
            
            return dto;
        }

        public async Task<bool> SaveSettingsAsync(List<AdPricingSettingDto> dtos)
        {
            var entities = dtos.Select(dto => new AdPricingSetting
            {
                Id = dto.Id ?? 0,
                PlacementType = dto.PlacementType,
                BillingType = dto.BillingType,
                MinUnits = dto.MinUnits,
                UnitPrice = dto.UnitPrice,
                MaxUnits = dto.MaxUnits,
                IsActive = dto.IsActive
            }).ToList();

            await _repository.SaveSettingsAsync(entities);
            return true;
        }
    }
}
