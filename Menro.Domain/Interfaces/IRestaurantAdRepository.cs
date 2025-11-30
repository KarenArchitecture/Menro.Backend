using Menro.Domain.Entities;

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
    }
}
