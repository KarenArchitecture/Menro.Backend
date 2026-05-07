using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.ShowAll.DTOs;
using Menro.Application.Features.ShowAll.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.ShowAll.Services.Implementations
{
    public class RecentOrderBrowseService : IRecentOrderBrowseService
    {
        private readonly IOrderRepository _orderRepository;

        public RecentOrderBrowseService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        private static RecentOrdersFoodCardDto Map(Food f)
        {
            var ratings = f.Ratings ?? new List<FoodRating>(0);
            var avg = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Score);

            return new RecentOrdersFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.ImageUrl ?? string.Empty,
                Rating = Math.Round(avg, 1),
                Voters = ratings.Count,

                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        public async Task<PagedResultDto<RecentOrdersFoodCardDto>> BrowseRecentOrderedFoodsAsync(
            string userId,
            int take = 6,
            string? cursor = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new PagedResultDto<RecentOrdersFoodCardDto>
                {
                    Items = new List<RecentOrdersFoodCardDto>(),
                    NextCursor = null,
                    HasMore = false
                };
            }

            take = Math.Clamp(take, 1, 24);

            // ✅ pass CancellationToken through to repository/EF Core
            var (foods, nextCursor, hasMore) =
                await _orderRepository.GetUserRecentlyOrderedFoodsCursorAsync(userId, take, cursor, ct);

            var items = (foods ?? new List<Food>())
                .Select(Map)
                .ToList();

            return new PagedResultDto<RecentOrdersFoodCardDto>
            {
                Items = items,
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }
    }
}