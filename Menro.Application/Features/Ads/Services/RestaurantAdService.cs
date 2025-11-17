using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Ads.DTOs;

namespace Menro.Application.Features.Ads.Services
{
    public class RestaurantAdService : IRestaurantAdService
    {
        private readonly IRestaurantAdRepository _repository;

        public RestaurantAdService(IRestaurantAdRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> CreateAsync(CreateRestaurantAdDto dto)
        {
            try
            {
                // 1. ایجاد مدل اولیه
                var ad = new RestaurantAd
                {
                    RestaurantId = dto.RestaurantId,
                    PlacementType = dto.PlacementType,
                    BillingType = dto.BillingType,
                    ImageFileName = dto.ImageFileName,
                    TargetUrl = dto.TargetUrl,
                    PurchasedUnits = dto.PurchasedUnits,
                    CommercialText = dto.CommercialText,

                    // مبلغ نهایی پرداخت‌شده (از درگاه/فرانت آمده)
                    Cost = dto.Cost
                };

                // 2. تعیین تاریخ شروع (همیشه لحظهٔ پرداخت)
                ad.StartDate = DateTime.UtcNow;

                // 3. تعیین تاریخ پایان بر اساس نوع پرداخت
                if (dto.BillingType == AdBillingType.PerDay)
                {
                    // نوع زمانی → پایان = تعداد روز
                    ad.EndDate = ad.StartDate.AddDays(dto.PurchasedUnits);
                }
                else
                {
                    // نوع کلیکی → معمولاً بدون محدودیت زمانی
                    // اما بهتر است حداکثر زمان داشته باشد (مثلاً ۶ ماه)
                    ad.EndDate = ad.StartDate.AddMonths(6);
                }

                // 4. ذخیره در دیتابیس
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
                IsActive = a.IsActive
            }).ToList();
        }
        public async Task IncrementConsumptionAsync(int adId, int amount = 1)
        {
            await _repository.UpdateConsumedUnitsAsync(adId, amount);
        }
    }
}
