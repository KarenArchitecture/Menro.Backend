using Menro.Domain.Enums;
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
                .AsNoTracking()
                .Where(a => a.PlacementType == placementType && a.IsActive)
                .ToListAsync();
        }

        public async Task UpsertSettingsAsync(List<AdPricingSetting> settings)
        {
            // اگر اشتباهاً چند placement مختلف آمد، پشتیبانی می‌کنیم
            var placements = settings.Select(s => s.PlacementType).Distinct().ToList();

            // همه رکوردهای مرتبط را یکجا بگیر
            var existing = await _context.AdPricingSettings
                .Where(x => placements.Contains(x.PlacementType))
                .ToListAsync();

            foreach (var s in settings)
            {
                var row = existing.FirstOrDefault(x =>
                    x.PlacementType == s.PlacementType &&
                    x.BillingType == s.BillingType);

                if (row == null)
                {
                    _context.AdPricingSettings.Add(new AdPricingSetting
                    {
                        PlacementType = s.PlacementType,
                        BillingType = s.BillingType,
                        MinUnits = s.MinUnits,
                        MaxUnits = s.MaxUnits,
                        UnitPrice = s.UnitPrice,
                        IsActive = s.IsActive
                    });
                }
                else
                {
                    row.MinUnits = s.MinUnits;
                    row.MaxUnits = s.MaxUnits;
                    row.UnitPrice = s.UnitPrice;
                    row.IsActive = s.IsActive;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
