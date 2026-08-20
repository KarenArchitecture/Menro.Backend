using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.Models;
using Menro.Application.Common.SD;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Application.Features.Users.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class AdminRestaurantService : IAdminRestaurantService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        private readonly IMediaStorageProvider _mediaStorage;

        public AdminRestaurantService(
            IUnitOfWork uow,
            IUserService userService,
            IGlobalDateTimeService globalDateTimeService,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
            _userService = userService;
            _globalDateTimeService = globalDateTimeService;
            _mediaStorage = mediaStorage;
        }
        #endregion
        // admin panel => restaurant management tab
        public async Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(RestaurantStatus status)
        {
            var restaurants = await _uow.Restaurant.GetRestaurantsListForAdminAsync(status);

            return restaurants.Select(r => new RestaurantListForAdminDto
            {
                Id = r.Id,
                Name = r.Name,
                PhoneNumber = r.ContactNumber ?? "",
                OwnerName = r.OwnerUser.FullName ?? "",
                Status = (int)r.Status,
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt),
            }).ToList();
        }


        public async Task<RestaurantDetailsForAdminDto?> GetRestaurantDetailsForAdminAsync(int id)
        {
            var r = await _uow.Restaurant.GetRestaurantDetailsForAdminAsync(id);
            if (r == null) return null;

            var entityId = r.Id.ToString();
            string openTime = r.OpenTime.ToString(@"hh\:mm");
            string closeTime = r.CloseTime.ToString(@"hh\:mm");
            string workingHours = $"{openTime} تا {closeTime}";

            return new RestaurantDetailsForAdminDto
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, entityId, MediaVariant.Resized),
                Address = r.Address ?? "",
                PhoneNumber = r.ContactNumber ?? "",
                WorkingHours = workingHours,
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt),

                NationalCode = r.NationalCode ?? "",
                BankAccountNumber = r.BankAccountNumber ?? "",
                ShebaNumber = r.ShebaNumber,

                OwnerName = r.OwnerUser?.FullName ?? "",
                CategoryName = r.RestaurantCategory?.Name ?? "",

                AverageRating = r.AverageRating,
                VotersCount = r.VotersCount,

                Status = (int)r.Status,
                RejectReason = r.RejectReason,
            };
        }
        public async Task<PagedResult<RestaurantOverviewDto>> GetRestaurantsOverviewAsync(string? search, int? categoryId, int page, int pageSize)
        {
            var query = _uow.Restaurant.QueryForAdmin(RestaurantStatus.Approved, search, categoryId);
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RestaurantOverviewDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    PhoneNumber = r.ContactNumber,
                    ImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, r.Id.ToString(), MediaVariant.Resized),
                })
                .ToListAsync();
            return new PagedResult<RestaurantOverviewDto>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Page = page,
            };
        }
        public async Task<bool> UpdateRestaurantStatusAsync(int restaurantId, RestaurantStatus status, string? rejectReason)
        {
            // ادمین نباید بتونه یه رستوران رو به حالت "در انتظار" برگردونه؛
            // Pending فقط حالت اولیه‌ی سیستمیه، نه یه تصمیم ادمین
            if (status == RestaurantStatus.Pending)
                return false;

            var restaurant = await _uow.Restaurant.GetByIdAsync(restaurantId);
            if (restaurant == null) return false;

            // جلوگیری از تصمیم‌گیری تکراری روی رستورانی که قبلاً approve/reject شده
            if (restaurant.Status != RestaurantStatus.Pending)
                return false;

            if (status == RestaurantStatus.Approved)
            {
                restaurant.Status = RestaurantStatus.Approved;
                restaurant.IsActive = true;
                restaurant.RejectReason = null;
                await _userService.AddRoleToUserAsync(restaurant.OwnerUserId, SD.Role_Owner);
            }
            else if (status == RestaurantStatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(rejectReason))
                    return false; // دلیل رد الزامیه، نمی‌شه رد کرد بدون توضیح

                restaurant.Status = RestaurantStatus.Rejected;
                restaurant.IsActive = false;
                restaurant.RejectReason = rejectReason.Trim();
            }

            var result = await _uow.SaveChangesAsync(); // یکسان‌سازی با بقیه‌ی UoW
            return result > 0;
        }
    }
}
