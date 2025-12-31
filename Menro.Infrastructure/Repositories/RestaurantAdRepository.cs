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


        // -----------------------------
        // Atomic billed consumption
        // -----------------------------

        public async Task<bool> TryConsumeUnitsAsync(int adId, int amount, AdBillingType billingType, DateTime nowUtc)
        {
            if (amount <= 0) return true;
            if (billingType == AdBillingType.PerDay) return true;

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

        public async Task<List<RestaurantAd>> GetActiveApprovedAdsAsync(
            AdPlacementType placementType,
            AdBillingType billingType,
            DateTime nowUtc)
        {
            return await _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.PlacementType == placementType &&
                    a.BillingType == billingType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    a.EndDate >= nowUtc
                )
                .Where(a => billingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits)
                .Include(a => a.Restaurant)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<RestaurantAd?> GetRandomActiveApprovedAdAsync(
    AdPlacementType placementType,
    AdBillingType billingType,
    DateTime nowUtc,
    IReadOnlyCollection<int> excludeRestaurantIds)
        {
            excludeRestaurantIds ??= Array.Empty<int>();

            var baseQ = _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.PlacementType == placementType &&
                    a.BillingType == billingType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    a.EndDate >= nowUtc &&
                    !excludeRestaurantIds.Contains(a.RestaurantId))
                .Where(a => billingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits);

            var count = await baseQ.CountAsync();
            if (count == 0) return null;

            var skip = Random.Shared.Next(0, count);

            return await baseQ
                .Include(a => a.Restaurant)
                .OrderBy(a => a.Id)
                .Skip(skip)
                .FirstOrDefaultAsync();
        }


        public async Task<int?> FindPairedAdIdAsync(int primaryAdId, AdBillingType pairedBillingType, DateTime nowUtc)
        {
            var primary = await _context.RestaurantAds
                .AsNoTracking()
                .Where(a => a.Id == primaryAdId)
                .Select(a => new { a.RestaurantId, a.PlacementType, a.ImageFileName, a.StartDate })
                .FirstOrDefaultAsync();

            if (primary == null) return null;

            return await _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.RestaurantId == primary.RestaurantId &&
                    a.PlacementType == primary.PlacementType &&
                    a.ImageFileName == primary.ImageFileName &&
                    a.BillingType == pairedBillingType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    a.EndDate >= nowUtc
                )
                .Where(a => pairedBillingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits)
                .OrderBy(a => Math.Abs(EF.Functions.DateDiffSecond(a.StartDate, primary.StartDate)))
                .ThenByDescending(a => a.CreatedAt)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();
        }

    }
}
