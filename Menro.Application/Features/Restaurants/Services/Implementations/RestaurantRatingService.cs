using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantRatingService : IRestaurantRatingService
    {
        private readonly IRestaurantRatingRepository _ratingRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantBannerService _restaurantBannerService;

        public RestaurantRatingService(
            IRestaurantRatingRepository ratingRepository,
            IRestaurantRepository restaurantRepository,
            IOrderRepository orderRepository,
            IRestaurantBannerService restaurantBannerService)
        {
            _ratingRepository = ratingRepository;
            _restaurantRepository = restaurantRepository;
            _orderRepository = orderRepository;
            _restaurantBannerService = restaurantBannerService;
        }

        public async Task<Result> RateRestaurantAsync(string userId, RateRestaurantDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure("برای امتیاز دادن ابتدا وارد حساب کاربری خود شوید.", ErrorCode.Invalid);

            if (dto.Score < 1 || dto.Score > 5)
                return Result.Failure("امتیاز باید بین ۱ تا ۵ باشد.", ErrorCode.Invalid);

            var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
            if (restaurant == null)
                return Result.Failure("رستوران یافت نشد.", ErrorCode.NotFound);

            // 🔒 قانون اصلی فیچر: فقط کسی که سفارش completed از این رستوران داشته می‌تونه رای بده
            var hasCompletedOrder = await _orderRepository.UserHasCompletedOrderAtRestaurantAsync(userId, dto.RestaurantId);
            if (!hasCompletedOrder)
                return Result.Failure(
                    "برای امتیاز دادن باید حداقل یک سفارش تکمیل‌شده از این رستوران داشته باشید.",
                    ErrorCode.Invalid);

            var existing = await _ratingRepository.GetByUserAndRestaurantAsync(userId, dto.RestaurantId);

            if (existing != null)
            {
                // ✏️ ویرایش رای قبلی — همون یک رای برای این کاربر آپدیت می‌شه
                existing.Score = dto.Score;
                existing.UpdatedAt = DateTime.UtcNow;
                await _ratingRepository.UpdateAsync(existing);
            }
            else
            {
                var rating = new RestaurantRating
                {
                    UserId = userId,
                    RestaurantId = dto.RestaurantId,
                    Score = dto.Score,
                    CreatedAt = DateTime.UtcNow
                };
                await _ratingRepository.AddAsync(rating);
            }

            await _ratingRepository.SaveChangesAsync();

            // 🔄 کش‌های صفحه‌ی خانه و بنر رستوران رو باطل کن تا امتیاز جدید همه‌جا دیده بشه
            _restaurantRepository.InvalidateRandomRestaurants();
            _restaurantRepository.InvalidateRestaurantBanner(restaurant.Slug);
            _restaurantBannerService.InvalidateCache(restaurant.Slug);

            return Result.Success();
        }

        public async Task<int?> GetUserRatingAsync(string userId, int restaurantId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var rating = await _ratingRepository.GetByUserAndRestaurantAsync(userId, restaurantId);
            return rating?.Score;
        }
    }
}