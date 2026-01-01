using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Rules;

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

            // فقط ترکیب‌های مجاز (برای اطمینان)
            list = list
                .Where(x => AdPricingRules.IsAllowed(x.PlacementType, x.BillingType))
                .ToList();

            return list.Select(x => new AdPricingSettingDto
            {
                Id = x.Id,
                PlacementType = x.PlacementType,
                BillingType = x.BillingType,
                MinUnits = x.MinUnits,
                MaxUnits = x.MaxUnits,
                UnitPrice = x.UnitPrice,
                IsActive = x.IsActive
            }).ToList();
        }

        public async Task SaveSettingsAsync(List<AdPricingSettingDto> dtos)
        {
            // 1) Validate قوانین و ورودی‌ها
            foreach (var dto in dtos)
            {
                if (!AdPricingRules.IsAllowed(dto.PlacementType, dto.BillingType))
                    throw new ArgumentException(
                        $"Invalid billing type '{dto.BillingType}' for placement '{dto.PlacementType}'.");

                if (dto.MinUnits < 0 || dto.MaxUnits < 0)
                    throw new ArgumentException("MinUnits/MaxUnits cannot be negative.");

                if (dto.MaxUnits < dto.MinUnits)
                    throw new ArgumentException("MaxUnits cannot be less than MinUnits.");

                if (dto.UnitPrice < 0)
                    throw new ArgumentException("UnitPrice cannot be negative.");
            }

            // 2) Map (Id مهم نیست)
            var entities = dtos.Select(dto => new AdPricingSetting
            {
                // Id را اینجا لازم نداریم
                PlacementType = dto.PlacementType,
                BillingType = dto.BillingType,
                MinUnits = dto.MinUnits,
                MaxUnits = dto.MaxUnits,
                UnitPrice = dto.UnitPrice,
                IsActive = dto.IsActive
            }).ToList();

            await _repository.UpsertSettingsAsync(entities);
        }
    }
}
