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
        Task<List<RestaurantAd>> GetActiveApprovedAdsAsync(AdPlacementType placementType, DateTime nowUtc);
        Task<RestaurantAd?> GetRandomActiveApprovedAdAsync(AdPlacementType placementType, DateTime nowUtc, IReadOnlyCollection<int> excludeAdIds);
        Task<bool> TryConsumeUnitsAsync(int adId, int amount, AdBillingType billingType, DateTime nowUtc);
    }
}
