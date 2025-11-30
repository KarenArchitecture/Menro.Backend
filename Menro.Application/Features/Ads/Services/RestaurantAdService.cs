using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Common.Interfaces;

namespace Menro.Application.Features.Ads.Services
{
    public class RestaurantAdService : IRestaurantAdService
    {
        private readonly IRestaurantAdRepository _repository;
        private readonly IGlobalDateTimeService _globalDateTimeService;

        public RestaurantAdService(IRestaurantAdRepository repository,
            IGlobalDateTimeService globalDateTimeService)
        {
            _repository = repository;
            _globalDateTimeService = globalDateTimeService;
        }

        public async Task<bool> CreateAsync(ReserveRestaurantAdDto dto)
        {
            try
            {
                var ad = new RestaurantAd
                {
                    RestaurantId = dto.RestaurantId,
                    PlacementType = dto.PlacementType,
                    BillingType = dto.BillingType,
                    ImageFileName = dto.ImageFileName,
                    TargetUrl = dto.TargetUrl,
                    CommercialText = dto.CommercialText,
                    PurchasedUnits = dto.PurchasedUnits,
                    Cost = dto.Cost,
                    Status = AdStatus.Pending,
                };

                ad.StartDate = DateTime.UtcNow;

                if (dto.BillingType == AdBillingType.PerDay)
                {
                    ad.EndDate = ad.StartDate.AddDays(dto.PurchasedUnits);
                }
                else
                {
                    ad.EndDate = ad.StartDate.AddMonths(6);
                }

                await _repository.AddAdAsync(ad);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<RestaurantAdListItemDto>> GetByRestaurantAsync(int restaurantId)
        {
            var ads = await _repository.GetByRestaurantAsync(restaurantId);

            return ads.Select(a => new RestaurantAdListItemDto
            {
                Id = a.Id,
                PlacementType = a.PlacementType,
                BillingType = a.BillingType,
                ImageUrl = a.ImageFileName,
                PurchasedUnits = a.PurchasedUnits,
                ConsumedUnits = a.ConsumedUnits,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
            }).ToList();
        }
        public async Task IncrementConsumptionAsync(int adId, int amount = 1)
        {
            await _repository.UpdateConsumedUnitsAsync(adId, amount);
        }
        public async Task<List<PendingAdDto>> GetPendingAdsAsync()
        {
            var ads = await _repository.GetPendingAdsAsync();

            return ads.Select(ad => new PendingAdDto
            {
                Id = ad.Id,
                RestaurantName = ad.Restaurant.Name,
                Placement = ad.PlacementType.ToString(),
                Billing = ad.BillingType.ToString(),
                Cost = ad.Cost,
                PurchasedUnits = ad.PurchasedUnits,
                TargetUrl = ad.TargetUrl ?? "--no link--",
                ImageUrl = ad.ImageFileName,
                CommercialText = ad.CommercialText ?? "--no commercial text--",
                CreatedAt = ad.StartDate,
                CreatedAtShamsi = _globalDateTimeService.ToPersianDateTimeString(ad.CreatedAt)
            }).ToList();
        }

        public async Task<bool> ApproveAdAsync(int adId)
        {
            var ad = await _repository.GetByIdAsync(adId);
            if (ad == null) return false;

            ad.Status = AdStatus.Approved;
            ad.AdminNotes = null;

            await _repository.UpdateAsync(ad);
            return true;
        }

        public async Task<bool> RejectAdAsync(RejectAdDto dto)
        {
            var ad = await _repository.GetByIdAsync(dto.Id);
            if (ad == null) return false;

            ad.Status = AdStatus.Rejected;
            ad.AdminNotes = dto.AdminNote;

            await _repository.UpdateAsync(ad);
            return true;
        }
    }
}
