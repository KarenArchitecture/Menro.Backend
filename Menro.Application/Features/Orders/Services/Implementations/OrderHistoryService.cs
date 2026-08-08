using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public OrderHistoryService(IOrderRepository orderRepository, IMediaStorageProvider mediaStorage)
        {
            _orderRepository = orderRepository;
            _mediaStorage = mediaStorage;
        }

        // 🔧 Needs the food's id, not just the image path — RestaurantFoodImage
        // is a category that requires an entityId to build the resized-variant
        // URL (same pattern as CartService/AdminOrderService). Calling GetUrl
        // without it throws "entity id الزامی است" and blows up the whole
        // /history endpoint with a 500.
        private string? BuildItemImageUrl(string? snapshot, string? liveImage, int foodId)
        {
            var raw = !string.IsNullOrWhiteSpace(snapshot) ? snapshot : liveImage;
            return string.IsNullOrWhiteSpace(raw)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, raw, foodId.ToString(), MediaVariant.Resized);
        }

        public async Task<List<UserOrderListItemDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            return orders.Select(o => new UserOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                InvoiceNumber = o.InvoiceNumber,
                RestaurantName = o.Restaurant?.Name ?? "",
                RestaurantLogoUrl = string.IsNullOrWhiteSpace(o.Restaurant?.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, o.Restaurant.LogoImageUrl, o.RestaurantId.ToString()),
                TableNumber = o.TableNumber,
                CreatedAt = o.CreatedAt,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                PreviewItems = o.OrderItems.Select(oi => new UserOrderPreviewItemDto
                {
                    FoodId = oi.FoodId,
                    ImageUrl = BuildItemImageUrl(oi.ImageUrlSnapshot, oi.Food?.ImageUrl, oi.FoodId),
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<PublicOrderDetailsDto?> GetOrderBillAsync(int orderId)
        {
            var order = await _orderRepository.GetPublicOrderDetailsAsync(orderId);
            if (order == null) return null;
            return new PublicOrderDetailsDto
            {
                Id = order.Id,
                RestaurantOrderNumber = order.RestaurantOrderNumber,
                InvoiceNumber = order.InvoiceNumber,
                RestaurantName = order.Restaurant?.Name ?? "",
                TableNumber = order.TableNumber,
                CreatedAt = order.CreatedAt,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new PublicOrderItemDto
                {
                    Name = oi.TitleSnapshot,
                    ImageUrl = BuildItemImageUrl(oi.ImageUrlSnapshot, oi.Food?.ImageUrl, oi.FoodId),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Rating = oi.Food?.AverageRating ?? 0,
                    Voters = oi.Food?.VotersCount ?? 0,
                    Addons = oi.Extras.Select(e => new PublicOrderAddonDto
                    {
                        Name = e.AddonTitleSnapshot,
                        Quantity = e.Quantity,
                        ExtraPrice = e.ExtraPrice
                    }).ToList()
                }).ToList()
            };
        }
    }
}