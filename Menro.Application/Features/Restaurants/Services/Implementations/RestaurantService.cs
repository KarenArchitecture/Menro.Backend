using Menro.Application.Features.Restaurants.DTOs;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Application.Extensions;
using Menro.Application.Common.Interfaces;
using Menro.Domain.Enums;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        private readonly IMediaStorageProvider _mediaStorage;

        public RestaurantService(
            IUnitOfWork uow,
            IGlobalDateTimeService globalDateTimeService,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
            _globalDateTimeService = globalDateTimeService;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<bool> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId)
        {
            // بررسی صحت داده‌ها (تکراری بودن نام؟ موجود بودن دسته‌بندی؟)
            var categoryExists = await _uow.RestaurantCategory
                .AnyAsync(c => c.Id == dto.RestaurantCategoryId);

            if (!categoryExists)
                return false;

            // adding restaurant
            try
            {
                var restaurant = new Restaurant
                {
                    Name = dto.RestaurantName,
                    Description = dto.RestaurantDescription,
                    Address = dto.RestaurantAddress,
                    //ContactNumber = dto.ContactNumber,
                    OpenTime = dto.RestaurantOpenTime,
                    CloseTime = dto.RestaurantCloseTime,
                    RestaurantCategoryId = dto.RestaurantCategoryId,
                    NationalCode = dto.OwnerNationalId,
                    BankAccountNumber = dto.RestaurantAccountNumber,
                    OwnerUserId = ownerUserId,
                    IsActive = true,
                    IsDeleted = false, // تا زمانی که توسط ادمین تأیید نشه
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Restaurant.AddAsync(restaurant);
                var result = await _uow.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<RestaurantCategoryDto>> GetRestaurantCategoriesAsync()
        {
            var categories = await _uow.RestaurantCategory.GetAllAsync();

            // مپ کردن به DTO
            var categoryDtos = categories.Select(c => new RestaurantCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return categoryDtos;

        }

        // ==========================================================
        //  Restaurant Category CRUD (admin panel - "دسته‌بندی انواع رستوران")
        // ==========================================================

        public async Task<RestaurantCategoryDto?> GetRestaurantCategoryByIdAsync(int id)
        {
            var category = await _uow.RestaurantCategory.GetByIdAsync(id);
            if (category == null) return null;

            return new RestaurantCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<(bool Success, string? Error)> CreateRestaurantCategoryAsync(CreateRestaurantCategoryDto dto)
        {
            var name = dto.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                return (false, "نام دسته‌بندی الزامی است");

            var isDuplicate = await _uow.RestaurantCategory.IsNameTakenAsync(name);
            if (isDuplicate)
                return (false, "دسته‌بندی با این نام قبلاً ثبت شده است");

            var category = new RestaurantCategory { Name = name };

            await _uow.RestaurantCategory.AddAsync(category);
            var result = await _uow.RestaurantCategory.SaveChangesAsync();

            return result
                ? (true, null)
                : (false, "خطا در ذخیره‌سازی دسته‌بندی");
        }

        public async Task<(bool Success, string? Error)> UpdateRestaurantCategoryAsync(UpdateRestaurantCategoryDto dto)
        {
            var category = await _uow.RestaurantCategory.GetByIdAsync(dto.Id);
            if (category == null)
                return (false, "دسته‌بندی یافت نشد");

            var name = dto.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                return (false, "نام دسته‌بندی الزامی است");

            var isDuplicate = await _uow.RestaurantCategory.IsNameTakenAsync(name, dto.Id);
            if (isDuplicate)
                return (false, "دسته‌بندی با این نام قبلاً ثبت شده است");

            category.Name = name;
            var result = await _uow.RestaurantCategory.SaveChangesAsync();

            return result
                ? (true, null)
                : (false, "خطا در بروزرسانی دسته‌بندی");
        }

        public async Task<(bool Success, string? Error)> DeleteRestaurantCategoryAsync(int id)
        {
            var category = await _uow.RestaurantCategory.GetByIdAsync(id);
            if (category == null)
                return (false, "دسته‌بندی یافت نشد");

            // don't allow deleting a category that restaurants still depend on
            var isInUse = await _uow.Restaurant.AnyAsync(r => r.RestaurantCategoryId == id);
            if (isInUse)
                return (false, "این دسته‌بندی توسط یک یا چند رستوران استفاده شده و قابل حذف نیست");

            await _uow.RestaurantCategory.DeleteAsync(category);
            var result = await _uow.RestaurantCategory.SaveChangesAsync();

            return result
                ? (true, null)
                : (false, "خطا در حذف دسته‌بندی");
        }

        public async Task<Restaurant?> GetRestaurantByIdAsync(int id)
        {
            var restaurant = await _uow.Restaurant.GetByIdAsync(id);
            return restaurant;
        }

        public async Task<string> GenerateUniqueSlugAsync(string name)
        {
            string baseSlug = name.TransliterateToEnglish(); // use extension
            string slug = baseSlug;
            int counter = 1;

            while (await _uow.Restaurant.SlugExistsAsync(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            return await _uow.Restaurant.GetRestaurantIdByUserIdAsync(userId);
        }
        public async Task<string> GetRestaurantName(int restaurantId)
        {
            return await _uow.Restaurant.GetRestaurantName(restaurantId);
        }


        // admin panel => restaurant management tab
        public async Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(RestaurantStatus status)
        {
            var restaurants = await _uow.Restaurant.GetRestaurantsListForAdminAsync(status);

            return restaurants.Select(r => new RestaurantListForAdminDto
            {
                Id = r.Id,
                Name = r.Name,
                PhoneNumber = r.OwnerUser.PhoneNumber ?? "",
                OwnerName = r.OwnerUser.FullName ?? "",
                Status = (int)r.Status,
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt),
            }).ToList();
        }


        public async Task<RestaurantDetailsForAdminDto?> GetRestaurantDetailsForAdminAsync(int id)
        {
            var r = await _uow.Restaurant.GetRestaurantDetailsForAdminAsync(id);
            if (r == null) return null;

            // formatting working hours
            string openTime = r.OpenTime.ToString(@"hh\:mm");
            string closeTime = r.CloseTime.ToString(@"hh\:mm");
            string workingHours = $"{openTime} تا {closeTime}";

            return new RestaurantDetailsForAdminDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? "",
                Address = r.Address ?? "",

                Type = r.RestaurantCategory?.Name ?? "",

                WorkingHours = workingHours,

                OwnerName = r.OwnerUser?.FullName ?? "",
                OwnerPhoneNumber = r.OwnerUser?.PhoneNumber ?? "",

                OwnerNationalId = r.NationalCode ?? "",
                OwnerBankAccount = r.BankAccountNumber ?? "",

                Status = (int)r.Status,
                RejectReason = r.RejectReason ?? "",
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt),

            };
        }


        public async Task<bool> ApproveRestaurantAsync(int restaurantId, bool approve)
        {
            var restaurant = await _uow.Restaurant.GetByIdAsync(restaurantId);
            if (restaurant == null) return false;
            restaurant.Status = RestaurantStatus.Approved;
            await _uow.Restaurant.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateRestaurantStatusAsync(int restaurantId, RestaurantStatus status, string? rejectReason)
        {
            var restaurant = await _uow.Restaurant.GetByIdAsync(restaurantId);
            if (restaurant == null) return false;

            restaurant.Status = status;

            if (status == RestaurantStatus.Rejected)
                restaurant.RejectReason = rejectReason?.Trim();
            else
                restaurant.RejectReason = null;

            await _uow.Restaurant.SaveChangesAsync();
            return true;
        }

        // restaurant profile

        public async Task<RestaurantProfileDto?> GetRestaurantProfileAsync(int id)
        {
            var r = await _uow.Restaurant.GetRestaurantProfileAsync(id);
            if (r == null) return null;

            return new RestaurantProfileDto
            {
                Id = r.Id,
                Name = r.Name,
                RestaurantCategoryId = r.RestaurantCategoryId,
                Address = r.Address,
                Description = r.Description,
                PhoneNumber = r.ContactNumber,
                BankAccountNumber = r.BankAccountNumber,
                OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                CloseTime = r.CloseTime.ToString(@"hh\:mm"),

                BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantHomeBanner, r.BannerImageUrl),

                ShopBannerImageUrl = string.IsNullOrWhiteSpace(r.ShopBannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantShopBanner, r.ShopBannerImageUrl),

                LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl),

                SubscriptionType = r.Subscription?.SubscriptionPlan.Name,
                SubscriptionDaysLeft = r.Subscription != null
                    ? (r.Subscription.EndDate - DateTime.UtcNow).Days
                    : 0
            };
        }

        public async Task UpdateRestaurantProfileAsync(UpdateRestaurantProfileDto dto)
        {
            var restaurant = await _uow.Restaurant.GetByIdAsync(dto.Id);
            if (restaurant == null)
                throw new Exception("Restaurant not found");

            /*----------------------------------------------
             *      Update simple fields
             *----------------------------------------------*/
            restaurant.Name = dto.Name;
            restaurant.RestaurantCategoryId = dto.RestaurantCategoryId;
            restaurant.Address = dto.Address;
            restaurant.Description = dto.Description;
            restaurant.BankAccountNumber = dto.BankAccountNumber;
            restaurant.ContactNumber = dto.PhoneNumber;

            restaurant.OpenTime = TimeSpan.Parse(dto.OpenTime);
            restaurant.CloseTime = TimeSpan.Parse(dto.CloseTime);

            /*----------------------------------------------
             *      Update / remove images
             *----------------------------------------------*/
            if (dto.HomeBanner != null)
                restaurant.BannerImageUrl = await UploadImageAsync(MediaCategory.RestaurantHomeBanner, restaurant.BannerImageUrl, dto.HomeBanner);
            else if (dto.RemoveHomeBanner)
                restaurant.BannerImageUrl = RemoveImage(MediaCategory.RestaurantHomeBanner, restaurant.BannerImageUrl);

            if (dto.ShopBanner != null)
                restaurant.ShopBannerImageUrl = await UploadImageAsync(MediaCategory.RestaurantShopBanner, restaurant.ShopBannerImageUrl, dto.ShopBanner);
            else if (dto.RemoveShopBanner)
                restaurant.ShopBannerImageUrl = RemoveImage(MediaCategory.RestaurantShopBanner, restaurant.ShopBannerImageUrl);

            if (dto.Logo != null)
                restaurant.LogoImageUrl = await UploadImageAsync(MediaCategory.RestaurantLogo, restaurant.LogoImageUrl, dto.Logo);
            else if (dto.RemoveLogo)
                restaurant.LogoImageUrl = RemoveImage(MediaCategory.RestaurantLogo, restaurant.LogoImageUrl);
            /*----------------------------------------------
             *      Save changes
             *----------------------------------------------*/
            await _uow.SaveChangesAsync();
        }

        /*----------------------------------------------
         *      MEDIA UPLOAD HELPERS
         *      هرکدوم صرفاً مسئول ذخیره‌ی یک نوع عکسه
         *      (حذف فایل قدیم به‌صورت خودکار توسط provider انجام میشه)
         *----------------------------------------------*/
        private async Task<string> UploadImageAsync(MediaCategory category, string? oldFileName, IFormFile file)
        {
            var result = await _mediaStorage.SaveAsync(category, file, oldFileName: oldFileName);
            return result.FileName;
        }

        private string? RemoveImage(MediaCategory category, string? oldFileName)
        {
            if (!string.IsNullOrEmpty(oldFileName))
                _mediaStorage.Delete(category, oldFileName);
            return null;
        }


    }
}
