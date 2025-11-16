using Menro.Domain.Entities;
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
                    !a.IsPaused &&
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
    }
}
