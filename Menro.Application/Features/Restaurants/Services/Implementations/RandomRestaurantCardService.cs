using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RandomRestaurantCardService : IRandomRestaurantCardService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public RandomRestaurantCardService(
            IRestaurantRepository restaurantRepository,
            IMediaStorageProvider mediaStorage)
        {
            _restaurantRepository = restaurantRepository;
            _mediaStorage = mediaStorage;
        }

        public async Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8)
        {
            var restaurants =
                await _restaurantRepository.GetRandomActiveApprovedWithDetailsAsync(count);
            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            return restaurants.Select(r =>
            {
                var entityId = r.Id.ToString();

                var rating = r.Ratings.Any()
                    ? Math.Round(r.Ratings.Average(x => x.Score), 1)
                    : 0;
                var voters = r.Ratings.Count;
                var discount = r.Discounts
                    .Where(d =>
                        d.IsActive &&
                        !d.IsDeleted &&
                        d.StartDate <= nowUtc &&
                        d.EndDate >= nowUtc)
                    .Select(d => (int?)d.Value)
                    .DefaultIfEmpty(null)
                    .Max();
                var isOpen = r.OpenTime <= r.CloseTime
                    ? nowTime >= r.OpenTime && nowTime < r.CloseTime
                    : nowTime >= r.OpenTime || nowTime < r.CloseTime;

                return new RestaurantCardDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
                    BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantHomeBanner, r.BannerImageUrl, entityId, MediaVariant.Thumbnail),
                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, entityId, MediaVariant.Thumbnail),
                    Rating = rating,
                    Voters = voters,
                    Discount = discount,
                    OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                    CloseTime = r.CloseTime.ToString(@"hh\:mm"),
                    IsOpen = isOpen,
                    Slug = r.Slug
                };
            }).ToList();
        }
    }
}