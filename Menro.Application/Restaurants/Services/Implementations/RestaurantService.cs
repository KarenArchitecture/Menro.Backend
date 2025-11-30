using Menro.Application.DTO;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Extensions;
using Menro.Application.Common.Interfaces;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _uow;
        private readonly IGlobalDateTimeService _globalDateTimeService;
        public RestaurantService(IUnitOfWork uow,
            IGlobalDateTimeService globalDateTimeService)
        {
            _uow = uow;
            _globalDateTimeService = globalDateTimeService;
        }

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
                    OpenTime = dto.RestaurantOpenTime,
                    CloseTime = dto.RestaurantCloseTime,
                    RestaurantCategoryId = dto.RestaurantCategoryId,
                    NationalCode = dto.OwnerNationalId,
                    BankAccountNumber = dto.RestaurantAccountNumber,
                    OwnerUserId = ownerUserId,
                    IsActive = true,
                    IsApproved = false, // تا زمانی که توسط ادمین تأیید نشه
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
        public async Task<List<RestaurantListForAdminDto>> GetRestaurantsListForAdminAsync(bool? approved)
        {
            var restaurants = await _uow.Restaurant.GetRestaurantsListForAdminAsync(approved);

            var list = restaurants.Select(r => new RestaurantListForAdminDto
            {
                Id = r.Id,
                Name = r.Name,
                PhoneNumber = r.OwnerUser.PhoneNumber ?? "",
                OwnerName = r.OwnerUser.FullName ?? "",
                IsApproved = r.IsApproved,
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt),
            }).ToList();

            return list;
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

                IsApproved = r.IsApproved,
                CreatedAt = _globalDateTimeService.ToPersianDateTimeString(r.CreatedAt)
            };
        }


        public async Task<bool> ApproveRestaurantAsync(int restaurantId, bool approve)
        {
            var restaurant = await _uow.Restaurant.GetByIdAsync(restaurantId);
            if (restaurant == null) return false;

            restaurant.IsApproved = approve;
            await _uow.Restaurant.SaveChangesAsync();
            return true;
        }

    }
}
