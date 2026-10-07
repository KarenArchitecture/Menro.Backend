using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Foods.DTOs;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediaStorageProvider _mediaStorage;
        private readonly IRestaurantRatingRepository _restaurantRatingRepository; // 🆕

        public OrderHistoryService(
            IOrderRepository orderRepository,
            IMediaStorageProvider mediaStorage,
            IRestaurantRatingRepository restaurantRatingRepository) // 🆕
        {
            _orderRepository = orderRepository;
            _mediaStorage = mediaStorage;
            _restaurantRatingRepository = restaurantRatingRepository;
        }

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

            // 🆕 یک‌بار برای همه‌ی رستوران‌های این تاریخچه، رای کاربر رو بگیر
            var restaurantIds = orders
                .Where(o => o.RestaurantId.HasValue)
                .Select(o => o.RestaurantId!.Value)
                .Distinct()
                .ToList();

            var userRatings = await _restaurantRatingRepository.GetUserRatingsForRestaurantsAsync(userId, restaurantIds);

            return orders.Select(o => new UserOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                InvoiceNumber = o.InvoiceNumber,
                RestaurantId = o.RestaurantId ?? 0,
                RestaurantSlug = o.Restaurant?.Slug ?? "",
                RestaurantName = o.Restaurant?.Name ?? "",
                RestaurantLogoUrl = string.IsNullOrWhiteSpace(o.Restaurant?.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, o.Restaurant.LogoImageUrl, o.RestaurantId.ToString()),
                TableLabel = o.TableLabel,
                CreatedAt = o.CreatedAt,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                UserRating = o.RestaurantId.HasValue && userRatings.TryGetValue(o.RestaurantId.Value, out var score)
                    ? score
                    : null,
                PreviewItems = o.OrderItems.Select(oi => new UserOrderPreviewItemDto
                {
                    FoodId = oi.FoodId,
                    ImageUrl = BuildItemImageUrl(oi.ImageUrlSnapshot, oi.Food?.ImageUrl, oi.FoodId),
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<List<FoodCardDto>> GetFrequentFoodsAtRestaurantAsync(
            string userId, string restaurantSlug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(restaurantSlug))
                return new List<FoodCardDto>();

            var foods = await _orderRepository.GetUserFrequentFoodsAtRestaurantAsync(userId, restaurantSlug, ct);

            // همان منطق RestaurantMenuService تا کارت‌ها دقیقاً مثل منو باشند
            return foods.Select(f =>
            {
                var displayPrice = f.Price;
                if (f.Variants != null && f.Variants.Any())
                {
                    var defaultVariant = f.Variants.FirstOrDefault(v => v.IsDefault == true)
                                       ?? f.Variants.FirstOrDefault(v => v.IsAvailable)
                                       ?? f.Variants.First();
                    displayPrice = defaultVariant.Price;
                }

                return new FoodCardDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Ingredients = f.Ingredients,
                    Price = displayPrice,
                    ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, f.Id.ToString(), MediaVariant.Thumbnail),
                    Rating = f.AverageRating,
                    Voters = f.VotersCount,
                    RestaurantName = f.Restaurant?.Name ?? string.Empty,
                    RestaurantCategory = f.CustomFoodCategory?.Name
                                         ?? f.GlobalFoodCategory?.Name
                                         ?? "نامشخص"
                };
            }).ToList();
        }

        public async Task<PublicOrderDetailsDto?> GetOrderBillAsync(int orderId, string? requestingUserId)
        {
            // بدون تغییر نسبت به قبل
            var order = await _orderRepository.GetPublicOrderDetailsAsync(orderId, requestingUserId);
            if (order == null) return null;
            return new PublicOrderDetailsDto
            {
                Id = order.Id,
                RestaurantOrderNumber = order.RestaurantOrderNumber,
                InvoiceNumber = order.InvoiceNumber,
                RestaurantName = order.Restaurant?.Name ?? "",
                TableLabel = order.TableLabel,
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