using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Order.Services.Implementations
{
    public class UsualOrdersService : IUsualOrdersService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public UsualOrdersService(IOrderRepository orderRepository, IMediaStorageProvider mediaStorage)
        {
            _orderRepository = orderRepository;
            _mediaStorage = mediaStorage;
        }

        public async Task<List<UsualOrderFoodDto>> GetUsualFoodsAsync(string userId, int restaurantId, int count = 12)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<UsualOrderFoodDto>();

            count = Math.Clamp(count, 1, 32);

            var foods = await _orderRepository.GetUserFrequentFoodsForRestaurantAsync(userId, restaurantId, count);
            if (foods.Count == 0)
                return new List<UsualOrderFoodDto>();

            return foods.Select(f => new UsualOrderFoodDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, f.Id.ToString(), MediaVariant.Resized),
                Price = f.Price,
                Rating = f.AverageRating,
                VotersCount = f.VotersCount,
                Variants = f.Variants.Select(v => new UsualOrderVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    IsDefault = v.IsDefault ?? false,
                    Addons = v.Addons.Select(a => new UsualOrderAddonDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice
                    }).ToList()
                }).ToList()
            }).ToList();
        }
    }
}