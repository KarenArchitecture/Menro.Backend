using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class AdminRestaurantService : IAdminRestaurantService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        private readonly IMediaStorageProvider _mediaStorage;

        public AdminRestaurantService(
            IUnitOfWork uow,
            IGlobalDateTimeService globalDateTimeService,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
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
    }
}
