using Menro.Domain.Entities;
using Menro.Domain.Enums;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantAdRepository
    {
        Task<bool> AddAdAsync(RestaurantAd ad);
        Task<List<RestaurantAd>> GetByRestaurantAsync(int restaurantId);
        Task<List<RestaurantAd>> GetActiveAdsAsync();
        Task<RestaurantAd?> GetByIdAsync(int id);
        Task UpdateConsumedUnitsAsync(int adId, int amount);
        Task<List<RestaurantAd>> GetPendingAdsAsync();
        Task<List<RestaurantAd>> GetHistoryAsync();
        Task<bool> UpdateAsync(RestaurantAd ad);

        //Public Face
        Task<bool> TryConsumeUnitsAsync(int adId, int amount, AdBillingType billingType, DateTime nowUtc);
        Task<List<RestaurantAd>> GetActiveApprovedAdsAsync(AdPlacementType placementType, AdBillingType billingType, DateTime nowUtc);
        Task<RestaurantAd?> GetRandomActiveApprovedAdAsync(AdPlacementType placementType, AdBillingType billingType, DateTime nowUtc, IReadOnlyCollection<int> excludeAdIds);
        Task<int?> FindPairedAdIdAsync(int primaryAdId, AdBillingType pairedBillingType, DateTime nowUtc);
    }
}
