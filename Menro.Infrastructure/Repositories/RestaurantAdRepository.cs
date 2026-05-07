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
            catch
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
                    (a.BillingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits)
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
            catch
            {
                return false;
            }
        }

        // -----------------------------
        // Public Face (independent billing rows)
        // -----------------------------
        private static bool IgnoreEndDate(AdPlacementType placementType)
    => placementType == AdPlacementType.FullscreenBanner;

        public async Task<bool> TryConsumeUnitsAsync(
            int adId,
            int amount,
            AdBillingType expectedBillingType,
            DateTime nowUtc)
        {
            if (amount <= 0) return true;

            // PerDay is not consumed here (handled elsewhere)
            if (expectedBillingType == AdBillingType.PerDay) return true;

            // NOTE: Banner has no expiration => ignore EndDate for banner rows
            var updated = await _context.RestaurantAds
                .Where(a =>
                    a.Id == adId &&
                    a.Status == AdStatus.Approved &&
                    a.BillingType == expectedBillingType &&
                    a.StartDate <= nowUtc &&
                    (a.PlacementType == AdPlacementType.FullscreenBanner || a.EndDate >= nowUtc) &&
                    a.ConsumedUnits + amount <= a.PurchasedUnits
                )
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(a => a.ConsumedUnits, a => a.ConsumedUnits + amount)
                );

            return updated == 1;
        }

        public async Task<List<RestaurantAd>> GetActiveApprovedAdsAsync(AdPlacementType placementType, DateTime nowUtc)
        {
            var ignoreEnd = IgnoreEndDate(placementType);

            return await _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.PlacementType == placementType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    (ignoreEnd || a.EndDate >= nowUtc) &&
                    (a.BillingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits)
                )
                .Include(a => a.Restaurant)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Optimized row-based random selection:
        /// - No COUNT+SKIP (O(N))
        /// - No ORDER BY NEWID() (full sort)
        /// - Uses a "random seek" strategy: pick a random target Id in [min..max] and take the first row >= target, wrapping around.
        /// </summary>
        public async Task<RestaurantAd?> GetRandomActiveApprovedAdAsync(
    AdPlacementType placementType,
    DateTime nowUtc,
    IReadOnlyCollection<int> excludeAdIds)
        {
            excludeAdIds ??= Array.Empty<int>();
            var ignoreEnd = IgnoreEndDate(placementType);

            IQueryable<RestaurantAd> baseQ = _context.RestaurantAds
                .AsNoTracking()
                .Where(a =>
                    a.PlacementType == placementType &&
                    a.Status == AdStatus.Approved &&
                    a.StartDate <= nowUtc &&
                    (a.BillingType == AdBillingType.PerDay || a.ConsumedUnits < a.PurchasedUnits)
                );

            if (!ignoreEnd)
                baseQ = baseQ.Where(a => a.EndDate >= nowUtc);

            if (excludeAdIds.Count != 0)
                baseQ = baseQ.Where(a => !excludeAdIds.Contains(a.Id));

            // one query min/max; safe if empty
            var range = await baseQ
                .Select(a => (long)a.Id)
                .GroupBy(_ => 1)
                .Select(g => new { Min = g.Min(), Max = g.Max() })
                .FirstOrDefaultAsync();

            if (range == null) return null;

            // Try a few random seeks to reduce "gap bias"
            for (int i = 0; i < 3; i++)
            {
                var target = (int)Random.Shared.NextInt64(range.Min, range.Max + 1);

                var picked = await baseQ
                    .Include(a => a.Restaurant)
                    .Where(a => a.Id >= target)
                    .OrderBy(a => a.Id)
                    .FirstOrDefaultAsync();

                if (picked != null) return picked;
            }

            // Wrap-around fallback (smallest Id)
            return await baseQ
                .Include(a => a.Restaurant)
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();
        }

    }
}
