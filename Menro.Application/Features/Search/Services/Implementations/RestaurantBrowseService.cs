using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Search.DTOs;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Search.Services.Implementations
{
    public class RestaurantBrowseService : IRestaurantBrowseService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public RestaurantBrowseService(
            IRestaurantRepository restaurantRepository,
            IMediaStorageProvider mediaStorage)
        {
            _restaurantRepository = restaurantRepository;
            _mediaStorage = mediaStorage;
        }

        public async Task<PagedResultDto<RestaurantCardDto>> GetRestaurantsPageAsync(
            int take = 20,
            int? cursorId = null)
        {
            take = Math.Clamp(take, 1, 50);
            if (cursorId.HasValue && cursorId.Value <= 0)
                cursorId = null;

            var list = await _restaurantRepository
                .GetActiveApprovedWithDetailsPageAsync(take, cursorId);

            var hasMore = list.Count > take;
            var slice = hasMore
                ? list.Take(take).ToList()
                : list;

            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            var items = slice.Select(r =>
            {
                var entityId = r.Id.ToString();

                double avgRating = r.Ratings?.Any() == true
                    ? Math.Round(r.Ratings.Average(rt => rt.Score), 1)
                    : 0;
                int voters = r.Ratings?.Count ?? 0;
                int? discountPercent = r.Discounts?
                    .Where(d =>
                        d.IsActive &&
                        d.Scope == DiscountScope.Restaurant &&
                        d.ValueType == DiscountValueType.Percent &&
                        d.StartDate <= nowUtc &&
                        d.EndDate >= nowUtc)
                    .Select(d => (int?)d.Value)
                    .DefaultIfEmpty(null)
                    .Max();
                bool isOpen = r.OpenTime <= r.CloseTime
                    ? nowTime >= r.OpenTime && nowTime < r.CloseTime
                    : nowTime >= r.OpenTime || nowTime < r.CloseTime;

                return new RestaurantCardDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.RestaurantCategory?.Name
                        ?? "بدون دسته‌بندی",
                    BannerImageUrl = string.IsNullOrWhiteSpace(r.ShopBannerImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantShopBanner, r.ShopBannerImageUrl, entityId, MediaVariant.Resized),
                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, entityId, MediaVariant.Thumbnail),
                    Rating = avgRating,
                    Voters = voters,
                    Discount = discountPercent,
                    OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                    CloseTime = r.CloseTime.ToString(@"hh\:mm"),
                    IsOpen = isOpen,
                    Slug = r.Slug
                };
            }).ToList();

            return new PagedResultDto<RestaurantCardDto>
            {
                Items = items,
                HasMore = hasMore,
                NextCursor = hasMore
                    ? slice.Last().Id.ToString()
                    : null
            };
        }
    }
}