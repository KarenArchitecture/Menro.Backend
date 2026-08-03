using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Application.Features.Search.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class RecentOrderBrowseService : IRecentOrderBrowseService
    {
        private readonly IOrderRepository _orderRepository;

        public RecentOrderBrowseService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Same mapping as UserRecentOrderCardService.Map — kept identical so
        // "recent-foods" and "recent-foods/browse" render the exact same
        // card shape on the client, just paginated differently.
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
            int take,
            string? cursor,
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

            var (foods, nextCursor, hasMore) = await _orderRepository
                .GetUserRecentlyOrderedFoodsCursorAsync(userId, take, cursor, ct);

            return new PagedResultDto<RecentOrdersFoodCardDto>
            {
                Items = foods.Select(Map).ToList(),
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }
    }
}