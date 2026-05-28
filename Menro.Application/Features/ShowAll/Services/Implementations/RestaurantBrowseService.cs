using Menro.Application.Common.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.ShowAll.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.ShowAll.Services.Implementations
{
    public class RestaurantBrowseService : IRestaurantBrowseService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IFileUrlService _fileUrlService;

        public RestaurantBrowseService(
            IRestaurantRepository restaurantRepository,
            IFileUrlService fileUrlService)
        {
            _restaurantRepository = restaurantRepository;
            _fileUrlService = fileUrlService;
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

                    BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                        ? _fileUrlService.BuildImageUrl("res-card-1.png")
                        : _fileUrlService.BuildImageUrl(r.BannerImageUrl),

                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? _fileUrlService.BuildRestaurantLogoUrl("logo-green.png")
                        : _fileUrlService.BuildRestaurantLogoUrl(r.LogoImageUrl),

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