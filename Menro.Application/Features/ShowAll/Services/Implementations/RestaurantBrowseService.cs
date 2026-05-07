using Menro.Application.DTO;
using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.ShowAll.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.ShowAll.Services.Implementations
{
    public class RestaurantBrowseService : IRestaurantBrowseService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantBrowseService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<PagedResultDto<RestaurantCardDto>> GetRestaurantsPageAsync(int take = 20, int? cursorId = null)
        {
            // Safety limits (fast + prevents abuse)
            take = Math.Clamp(take, 1, 50);
            if (cursorId.HasValue && cursorId.Value <= 0) cursorId = null;

            var list = await _restaurantRepository.GetActiveApprovedWithDetailsPageAsync(take, cursorId);

            var hasMore = list.Count > take;
            var slice = hasMore ? list.Take(take).ToList() : list;

            var nowTime = DateTime.Now.TimeOfDay;
            var nowUtc = DateTime.UtcNow;

            var items = slice.Select(r =>
            {
                double avgRating = r.Ratings?.Any() == true
                    ? Math.Round(r.Ratings.Average(rt => rt.Score), 1)
                    : 0;

                int voters = r.Ratings?.Count ?? 0;

                int? discountPercent = r.Discounts?
                    .Where(d => d.StartDate <= nowUtc && d.EndDate >= nowUtc)
                    .Select(d => (int?)d.Percent)
                    .DefaultIfEmpty(null)
                    .Max();

                bool isOpen = r.OpenTime <= r.CloseTime
                    ? nowTime >= r.OpenTime && nowTime < r.CloseTime
                    : nowTime >= r.OpenTime || nowTime < r.CloseTime;

                return new RestaurantCardDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.RestaurantCategory?.Name ?? "بدون دسته‌بندی",
                    BannerImageUrl = string.IsNullOrWhiteSpace(r.BannerImageUrl)
                        ? "/img/res-cards.png"
                        : r.BannerImageUrl,
                    LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                        ? "/img/res-slider.png"
                        : r.LogoImageUrl,
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
                NextCursor = hasMore ? slice.Last().Id.ToString() : null
            };
        }
    }
}
