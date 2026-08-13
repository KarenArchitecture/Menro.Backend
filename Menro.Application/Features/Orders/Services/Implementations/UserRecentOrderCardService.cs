using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
namespace Menro.Application.Features.Order.Services.Implementations
{
    public class UserRecentOrderCardService : IUserRecentOrderCardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public UserRecentOrderCardService(IOrderRepository orderRepository,
            IMediaStorageProvider mediaStorage)
        {
            _orderRepository = orderRepository;
            _mediaStorage = mediaStorage;
        }

        private RecentOrdersFoodCardDto Map(Food f)
        {
            var ratings = f.Ratings ?? new List<FoodRating>(0);
            var avg = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Score);
            return new RecentOrdersFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, f.Id.ToString(), MediaVariant.Thumbnail),
                Rating = Math.Round(avg, 1),
                Voters = ratings.Count,
                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        public async Task<List<RecentOrdersFoodCardDto>> GetUserRecentOrderedFoodsAsync(string userId, int count = 8)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<RecentOrdersFoodCardDto>();
            if (count <= 0) count = 8;
            if (count > 32) count = 32;

            var foods = await _orderRepository.GetUserRecentlyOrderedFoodsAsync(userId, count);
            if (foods == null || foods.Count == 0)
                return new List<RecentOrdersFoodCardDto>();

            return foods.Select(Map).ToList();
        }
    }
}