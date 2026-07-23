using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Menro.Application.Features.Ads.DTOs;

namespace Menro.Application.Features.Ads.Services
{
    public interface IRestaurantAdService
    {
        Task<string> UploadAdImageAsync(IFormFile file, AdPlacementType placementType);
        Task<bool> CreateAsync(ReserveRestaurantAdDto dto);
        Task<List<RestaurantAdListItemDto>> GetByRestaurantAsync(int restaurantId);
        Task IncrementConsumptionAsync(int adId, int amount = 1);
        Task<List<PendingAdDto>> GetPendingAdsAsync();
        Task<bool> ApproveAdAsync(int adId);
        Task<bool> RejectAdAsync(RejectAdDto dto);
        Task<List<HistoryAdDto>> GetHistoryAsync();
    }
}