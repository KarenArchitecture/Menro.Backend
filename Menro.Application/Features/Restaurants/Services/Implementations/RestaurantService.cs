using Menro.Application.Features.Restaurants.DTOs;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Application.Extensions;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;
using Menro.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IMediaStorageProvider _mediaStorage;

        public RestaurantService(
            IUnitOfWork uow,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<(bool Success, string? Error)> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId)
        {
            var categoryExists = await _uow.RestaurantCategory
                .AnyAsync(c => c.Id == dto.RestaurantCategoryId);
            if (!categoryExists)
                return (false, "دسته‌بندی انتخاب‌شده معتبر نیست.");

            var existing = await _uow.Restaurant.GetByOwnerUserIdAsync(ownerUserId);

            if (existing == null)
                return await CreateNewRestaurantAsync(dto, ownerUserId);

            if (existing.Status == RestaurantStatus.Approved)
                return (false, "شما از قبل یک رستوران تاییدشده دارید.");

            if (existing.Status == RestaurantStatus.Pending)
                return (false, "شما یک درخواست در حال بررسی دارید.");

            // فقط اینجا، یعنی existing.Status == Rejected، اجازه‌ی بازنویسی داریم
            return await ResubmitRejectedRestaurantAsync(existing, dto);
        }

        private async Task<(bool Success, string? Error)> CreateNewRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId)
        {
            try
            {
                var restaurant = new Restaurant
                {
                    Name = dto.RestaurantName,
                    Description = dto.RestaurantDescription,
                    Address = dto.RestaurantAddress,
                    ContactNumber = dto.ContactNumber,
                    OpenTime = dto.RestaurantOpenTime,
                    CloseTime = dto.RestaurantCloseTime,
                    RestaurantCategoryId = dto.RestaurantCategoryId,
                    NationalCode = dto.OwnerNationalId,
                    BankAccountNumber = dto.RestaurantAccountNumber,
                    OwnerUserId = ownerUserId,
                    Status = RestaurantStatus.Pending,
                    IsActive = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Restaurant.AddAsync(restaurant);
                var result = await _uow.SaveChangesAsync();
                return (result > 0, result > 0 ? null : "ثبت رستوران با خطا مواجه شد.");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Restaurants_OwnerUserId") == true)
            {
                return (false, "شما قبلاً یک رستوران ثبت کرده‌اید.");
            }
            catch (Exception)
            {
                return (false, "ثبت رستوران با خطا مواجه شد.");
            }
        }

        private async Task<(bool Success, string? Error)> ResubmitRejectedRestaurantAsync(Restaurant existing, RegisterRestaurantDto dto)
        {
            // Safety guard صریح: این متد فقط حق داره رکورد Rejected رو دست بزنه.
            // اگه به هر دلیلی (باگ فراخوانی، تغییر بعدی کد) با غیر از این صدا زده بشه،
            // به‌جای پاک کردن بی‌صدای اطلاعات یه کسب‌وکار فعال، exception می‌ده.
            if (existing.Status != RestaurantStatus.Rejected)
                throw new InvalidOperationException(
                    $"ResubmitRejectedRestaurantAsync can only be called for a Rejected restaurant. Current status: {existing.Status}");

            existing.Name = dto.RestaurantName;
            existing.Description = dto.RestaurantDescription;
            existing.Address = dto.RestaurantAddress;
            existing.ContactNumber = dto.ContactNumber;
            existing.OpenTime = dto.RestaurantOpenTime;
            existing.CloseTime = dto.RestaurantCloseTime;
            existing.RestaurantCategoryId = dto.RestaurantCategoryId;
            existing.NationalCode = dto.OwnerNationalId;
            existing.BankAccountNumber = dto.RestaurantAccountNumber;
            existing.Status = RestaurantStatus.Pending;
            existing.IsActive = false;
            existing.RejectReason = null;
            existing.CreatedAt = DateTime.UtcNow;

            var result = await _uow.SaveChangesAsync();
            return (result > 0, result > 0 ? null : "ثبت رستوران با خطا مواجه شد.");
        }

        public async Task<MyRestaurantStatusDto?> GetOwnerRestaurantStatusAsync(string ownerUserId)
        {
            var restaurant = await _uow.Restaurant.GetByOwnerUserIdAsync(ownerUserId);
            if (restaurant == null) return null;

            return new MyRestaurantStatusDto
            {
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Name,
                Status = (int)restaurant.Status,
                RejectReason = restaurant.RejectReason,
            };
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
            string baseSlug = SlugHelper.GenerateSlug(name);
            string slug = baseSlug;
            int counter = 2;

            while (await _uow.Restaurant.SlugExistsAsync(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task<bool> IsSlugAvailableAsync(string slug, int excludeRestaurantId)
        {
            var normalized = SlugHelper.NormalizeAscii(slug);
            if (string.IsNullOrEmpty(normalized))
                return false;

            var exists = await _uow.Restaurant.SlugExistsAsync(normalized, excludeRestaurantId);
            return !exists;
        }

        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            return await _uow.Restaurant.GetRestaurantIdByUserIdAsync(userId);
        }
        public async Task<string> GetRestaurantName(int restaurantId)
        {
            return await _uow.Restaurant.GetRestaurantName(restaurantId);
        }

        // restaurant profile
        public async Task<RestaurantProfileDto?> GetRestaurantProfileAsync(int id)
        {
            var r = await _uow.Restaurant.GetRestaurantProfileAsync(id);
            if (r == null) return null;

            var entityId = r.Id.ToString();

            return new RestaurantProfileDto
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                RestaurantCategoryId = r.RestaurantCategoryId,
                Address = r.Address,
                NationalCode = r.NationalCode,
                Description = r.Description,
                PhoneNumber = r.ContactNumber,
                BankAccountNumber = r.BankAccountNumber,
                ShebaNumber = r.ShebaNumber,
                OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                CloseTime = r.CloseTime.ToString(@"hh\:mm"),

                BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantHomeBanner, r.BannerImageUrl, entityId, MediaVariant.Resized),

                ShopBannerImageUrl = string.IsNullOrWhiteSpace(r.ShopBannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantShopBanner, r.ShopBannerImageUrl, entityId, MediaVariant.Resized),

                LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, entityId, MediaVariant.Resized),

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

            var entityId = restaurant.Id.ToString();

            if (!string.IsNullOrWhiteSpace(dto.Slug))
            {
                var normalizedSlug = SlugHelper.NormalizeAscii(dto.Slug);

                if (!string.Equals(normalizedSlug, restaurant.Slug, StringComparison.OrdinalIgnoreCase))
                {
                    var isAvailable = await IsSlugAvailableAsync(normalizedSlug, restaurant.Id);
                    if (!isAvailable)
                        throw new InvalidOperationException("این اسلاگ قبلاً استفاده شده است.");

                    restaurant.Slug = normalizedSlug;
                }
            }

            restaurant.Name = dto.Name;

            restaurant.RestaurantCategoryId = dto.RestaurantCategoryId;
            restaurant.Address = dto.Address;
            restaurant.Description = dto.Description;
            restaurant.NationalCode = dto.NationalCode;
            restaurant.ShebaNumber = dto.ShebaNumber;
            restaurant.BankAccountNumber = dto.BankAccountNumber;
            restaurant.ContactNumber = dto.PhoneNumber;

            restaurant.OpenTime = TimeSpan.Parse(dto.OpenTime);
            restaurant.CloseTime = TimeSpan.Parse(dto.CloseTime);

            if (dto.HomeBanner != null)
                restaurant.BannerImageUrl = await UploadImageAsync(MediaCategory.RestaurantHomeBanner, entityId, restaurant.BannerImageUrl, dto.HomeBanner);
            else if (dto.RemoveHomeBanner)
                restaurant.BannerImageUrl = RemoveImage(MediaCategory.RestaurantHomeBanner, entityId, restaurant.BannerImageUrl);

            if (dto.ShopBanner != null)
                restaurant.ShopBannerImageUrl = await UploadImageAsync(MediaCategory.RestaurantShopBanner, entityId, restaurant.ShopBannerImageUrl, dto.ShopBanner);
            else if (dto.RemoveShopBanner)
                restaurant.ShopBannerImageUrl = RemoveImage(MediaCategory.RestaurantShopBanner, entityId, restaurant.ShopBannerImageUrl);

            if (dto.Logo != null)
                restaurant.LogoImageUrl = await UploadImageAsync(MediaCategory.RestaurantLogo, entityId, restaurant.LogoImageUrl, dto.Logo);
            else if (dto.RemoveLogo)
                restaurant.LogoImageUrl = RemoveImage(MediaCategory.RestaurantLogo, entityId, restaurant.LogoImageUrl);

            await _uow.SaveChangesAsync();
        }

        /*----------------------------------------------
         *      MEDIA UPLOAD HELPERS
         *      هرکدوم صرفاً مسئول ذخیره‌ی یک نوع عکسه
         *      (حذف فایل قدیم به‌صورت خودکار توسط provider انجام میشه)
         *----------------------------------------------*/
        private async Task<string> UploadImageAsync(MediaCategory category, string entityId, string? oldFileName, IFormFile file)
        {
            var result = await _mediaStorage.SaveAsync(category, file, entityId, oldFileName: oldFileName);
            return result.FileName;
        }

        private string? RemoveImage(MediaCategory category, string entityId, string? oldFileName)
        {
            if (!string.IsNullOrEmpty(oldFileName))
                _mediaStorage.Delete(category, oldFileName, entityId);
            return null;
        }


    }
}
