using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class AdPricingSettingRepository : IAdPricingSettingRepository
    {
        private readonly MenroDbContext _context;

        public AdPricingSettingRepository(MenroDbContext context)
        {
            _context = context;
        }
        public async Task<List<AdPricingSetting>> GetActiveSettingsAsync(AdPlacementType placementType)
        {
            return await _context.AdPricingSettings
                .Where(a => a.PlacementType == placementType && a.IsActive)
                .ToListAsync();
        }

        public async Task SaveSettingsAsync(List<AdPricingSetting> settings)
        {
            foreach (var setting in settings)
            {
                if (setting.Id == 0)
                {
                    // CREATE
                    _context.AdPricingSettings.Add(setting);
                }
                else
                {
                    // UPDATE
                    var existing = await _context.AdPricingSettings
                        .FirstOrDefaultAsync(x => x.Id == setting.Id);

                    if (existing != null)
                    {
                        existing.PlacementType = setting.PlacementType;
                        existing.BillingType = setting.BillingType;
                        existing.MinUnits = setting.MinUnits;
                        existing.MaxUnits = setting.MaxUnits;
                        existing.UnitPrice = setting.UnitPrice;
                        existing.IsActive = setting.IsActive;
                    }
                    else
                    {
                        _context.AdPricingSettings.Add(setting);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
