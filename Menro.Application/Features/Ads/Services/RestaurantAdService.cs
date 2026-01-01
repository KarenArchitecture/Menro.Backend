using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Menro.Domain.Enums;

namespace Menro.Application.Features.Ads.Services
{
    public class RestaurantAdService : IRestaurantAdService
    {
        private readonly IRestaurantAdRepository _repository;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantAdService(IRestaurantAdRepository repository,
            IGlobalDateTimeService globalDateTimeService,
            IRestaurantRepository restaurantRepository)
        {
            _repository = repository;
            _globalDateTimeService = globalDateTimeService;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<bool> CreateAsync(ReserveRestaurantAdDto dto)
        {
            try
            {
                Restaurant? restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
                if (restaurant == null) return false;

                var ad = new RestaurantAd
                {
                    RestaurantId = dto.RestaurantId,
                    PlacementType = dto.PlacementType,
                    BillingType = dto.BillingType,
                    ImageFileName = dto.ImageFileName,
                    TargetUrl = restaurant.Slug,
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
                    // هر نوع تبلیغات دیگری، تا حداکثر شش ماه اعتبار دارد
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
        public async Task<List<HistoryAdDto>> GetHistoryAsync()
        {
            var ads = await _repository.GetHistoryAsync();

            return ads.Select(ad => new HistoryAdDto
            {
                Id = ad.Id,
                RestaurantName = ad.Restaurant.Name,
                Placement = ad.PlacementType.ToString(),
                Billing = ad.BillingType.ToString(),
                Cost = ad.Cost,
                PurchasedUnits = ad.PurchasedUnits,
                TargetUrl = ad.TargetUrl,
                ImageUrl = ad.ImageFileName,
                CommercialText = ad.CommercialText,
                CreatedAt = ad.CreatedAt,
                CreatedAtShamsi = _globalDateTimeService.ConvertToPersian(ad.CreatedAt),
                Status = ad.Status.ToString(),
                AdminNotes = ad.AdminNotes ?? ""
            }).ToList();
        }



    }
}
