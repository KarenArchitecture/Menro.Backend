using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantAdRepository : IRestaurantAdRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantAdRepository(MenroDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddAdAsync(RestaurantAd ad)
        {
            try
            {
                _context.RestaurantAds.Add(ad);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<RestaurantAd>> GetByRestaurantAsync(int restaurantId)
        {
            return await _context.RestaurantAds
                .Where(x => x.RestaurantId == restaurantId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<RestaurantAd>> GetActiveAdsAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.RestaurantAds
                .Where(a =>
                    a.StartDate <= now &&
                    a.EndDate >= now &&
                    (a.BillingType == AdBillingType.PerDay
                        || a.ConsumedUnits < a.PurchasedUnits)
                )
                .ToListAsync();
        }

        public async Task<RestaurantAd?> GetByIdAsync(int id)
        {
            return await _context.RestaurantAds.FindAsync(id);
        }

        public async Task UpdateConsumedUnitsAsync(int adId, int amount)
        {
            var ad = await _context.RestaurantAds.FindAsync(adId);
            if (ad == null) return;

            ad.ConsumedUnits += amount;
            await _context.SaveChangesAsync();
        }
        public async Task<List<RestaurantAd>> GetPendingAdsAsync()
        {
            return await _context.RestaurantAds
                .Include(x => x.Restaurant)
                .Where(x => x.Status == AdStatus.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<RestaurantAd>> GetHistoryAsync()
        {
            return await _context.RestaurantAds
                .Include(x => x.Restaurant)
                .Where(x => x.Status == AdStatus.Approved || x.Status == AdStatus.Rejected)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task<bool> UpdateAsync(RestaurantAd ad)
        {
            if (ad == null) return false;
            try
            {
                _context.RestaurantAds.Update(ad);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // -----------------------------
        // ✅ NEW: Public Delivery Queries
        // -----------------------------

        public async Task<List<RestaurantAd>> GetActiveApprovedAdsAsync(AdPlacementType placementType, DateTime nowUtc)
        {
            return await BuildActiveApprovedQuery(placementType, nowUtc)
                .Include(a => a.Restaurant)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<RestaurantAd?> GetRandomActiveApprovedAdAsync(
            AdPlacementType placementType,
            DateTime nowUtc,
            IReadOnlyCollection<int> excludeAdIds)
        {
            excludeAdIds ??= Array.Empty<int>();

            return await BuildActiveApprovedQuery(placementType, nowUtc)
                .Include(a => a.Restaurant)
                .Where(a => !excludeAdIds.Contains(a.Id))
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        // -----------------------------
        // Atomic billed consumption
        // -----------------------------

        public async Task<bool> TryConsumeUnitsAsync(int adId, int amount, AdBillingType billingType, DateTime nowUtc)
        {
            if (amount <= 0) return true;

            // PerDay is time-based; do not consume units for billing
            if (billingType == AdBillingType.PerDay) return true;

            // This guarantees we don't overshoot PurchasedUnits under concurrency.
            // EF Core 7/8 supports ExecuteUpdateAsync (recommended).
            var updated = await _context.RestaurantAds
                .Where(a =>
                    a.Id == adId &&
                    a.Status == AdStatus.Approved &&
                    a.BillingType == billingType &&
                    a.StartDate <= nowUtc &&
                    a.EndDate >= nowUtc &&
                    a.ConsumedUnits + amount <= a.PurchasedUnits
                )
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(a => a.ConsumedUnits, a => a.ConsumedUnits + amount)
                );

            return updated == 1;
        }

        private IQueryable<RestaurantAd> BuildActiveApprovedQuery(AdPlacementType placementType, DateTime nowUtc)
        {
            return _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.PlacementType == placementType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    a.EndDate >= nowUtc
                )
                // For PerClick and PerView, enforce remaining units
                .Where(a => a.BillingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits);
        }
    }
}
