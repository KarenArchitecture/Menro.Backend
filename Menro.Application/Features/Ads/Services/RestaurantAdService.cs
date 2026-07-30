using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Common.Interfaces;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Ads.Services
{
    public class RestaurantAdService : IRestaurantAdService
    {
        #region DI
        private readonly IRestaurantAdRepository _repository;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public RestaurantAdService(IRestaurantAdRepository repository,
            IGlobalDateTimeService globalDateTimeService,
            IRestaurantRepository restaurantRepository,
            IMediaStorageProvider mediaStorage)
        {
            _repository = repository;
            _globalDateTimeService = globalDateTimeService;
            _restaurantRepository = restaurantRepository;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<string> UploadAdImageAsync(IFormFile file, AdPlacementType placementType, int restaurantId)
        {
            var result = await _mediaStorage.SaveAsync(AdMediaCategoryResolver.Resolve(placementType), file, restaurantId.ToString());
            return result.FileName;
        }

        public async Task<bool> CreateAsync(ReserveRestaurantAdDto dto)
        {
            Restaurant? restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
            if (restaurant == null) return false;

            var category = AdMediaCategoryResolver.Resolve(dto.PlacementType);
            var uploadResult = await _mediaStorage.SaveAsync(category, dto.Image, dto.RestaurantId.ToString());

            var ad = new RestaurantAd
            {
                RestaurantId = dto.RestaurantId,
                PlacementType = dto.PlacementType,
                BillingType = dto.BillingType,
                ImageFileName = uploadResult.FileName,
                TargetUrl = restaurant.Slug,
                CommercialText = dto.CommercialText,
                PurchasedUnits = dto.PurchasedUnits,
                Cost = dto.Cost,
                Status = AdStatus.Pending,
                StartDate = DateTime.UtcNow,
            };

            ad.EndDate = dto.BillingType == AdBillingType.PerDay ? ad.StartDate.AddDays(dto.PurchasedUnits) : ad.StartDate.AddMonths(6);

            try
            {
                await _repository.AddAdAsync(ad);
            }
            catch
            {
                _mediaStorage.Delete(category, uploadResult.FileName, dto.RestaurantId.ToString());
                throw;
            }

            return true;
        }
        public async Task<List<RestaurantAdListItemDto>> GetByRestaurantAsync(int restaurantId)
        {
            var ads = await _repository.GetByRestaurantAsync(restaurantId);

            return ads.Select(a => new RestaurantAdListItemDto
            {
                Id = a.Id,
                PlacementType = a.PlacementType,
                BillingType = a.BillingType,
                ImageUrl = _mediaStorage.GetUrl(AdMediaCategoryResolver.Resolve(a.PlacementType), a.ImageFileName, a.RestaurantId.ToString()),
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
                ImageUrl = _mediaStorage.GetUrl(AdMediaCategoryResolver.Resolve(ad.PlacementType), ad.ImageFileName, ad.RestaurantId.ToString()),
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
                ImageUrl = _mediaStorage.GetUrl(AdMediaCategoryResolver.Resolve(ad.PlacementType), ad.ImageFileName, ad.RestaurantId.ToString()),
                CommercialText = ad.CommercialText,
                CreatedAt = ad.CreatedAt,
                CreatedAtShamsi = _globalDateTimeService.ConvertToPersian(ad.CreatedAt),
                Status = ad.Status.ToString(),
                AdminNotes = ad.AdminNotes ?? ""
            }).ToList();
        }

        // my ads
        public async Task<List<MyAdItemDto>> GetMyPendingAdsAsync(int restaurantId)
        {
            var ads = await _repository.GetByRestaurantAsync(restaurantId);

            return ads
                .Where(a => a.Status == AdStatus.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToMyAdItemDto)
                .ToList();
        }

        public async Task<List<MyAdItemDto>> GetMyActiveAdsAsync(int restaurantId)
        {
            var now = DateTime.UtcNow;
            var ads = await _repository.GetByRestaurantAsync(restaurantId);

            return ads
                .Where(a => a.Status == AdStatus.Approved
                         && a.EndDate >= now
                         && a.ConsumedUnits < a.PurchasedUnits)
                .OrderBy(a => a.EndDate) // زودتر تمام‌شونده‌ها اول
                .Select(MapToMyAdItemDto)
                .ToList();
        }

        public async Task<List<MyAdItemDto>> GetMyHistoryAdsAsync(int restaurantId)
        {
            var now = DateTime.UtcNow;
            var ads = await _repository.GetByRestaurantAsync(restaurantId);

            return ads
                .Where(a => a.Status == AdStatus.Rejected
                         || (a.Status == AdStatus.Approved
                             && (a.EndDate < now || a.ConsumedUnits >= a.PurchasedUnits)))
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToMyAdItemDto)
                .ToList();
        }

        private MyAdItemDto MapToMyAdItemDto(RestaurantAd ad) => new MyAdItemDto
        {
            Id = ad.Id,
            Placement = ad.PlacementType.ToString(),
            Billing = ad.BillingType.ToString(),
            Cost = ad.Cost,
            PurchasedUnits = ad.PurchasedUnits,
            ConsumedUnits = ad.ConsumedUnits,
            ImageUrl = _mediaStorage.GetUrl(
                AdMediaCategoryResolver.Resolve(ad.PlacementType), ad.ImageFileName, ad.RestaurantId.ToString()),
            CommercialText = ad.CommercialText,
            TargetUrl = ad.TargetUrl,
            Status = ad.Status.ToString(),
            AdminNotes = ad.AdminNotes,
            CreatedAt = ad.CreatedAt,
            CreatedAtShamsi = _globalDateTimeService.ToPersianDateTimeString(ad.CreatedAt),
            StartDate = ad.StartDate,
            EndDate = ad.EndDate,
            StartDateShamsi = _globalDateTimeService.ToPersianDateTimeString(ad.StartDate),
            EndDateShamsi = _globalDateTimeService.ToPersianDateTimeString(ad.EndDate),
        };

    }
}
