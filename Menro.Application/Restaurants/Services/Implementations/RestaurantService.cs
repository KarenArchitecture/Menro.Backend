using Menro.Application.DTO;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Menro.Application.Extensions;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _uow;
        public RestaurantService(IUnitOfWork uow)
        {
            _uow = uow;
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

    }
}
