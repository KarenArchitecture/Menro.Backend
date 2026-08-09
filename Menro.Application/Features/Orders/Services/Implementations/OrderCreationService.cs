using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Foods.Services.Interfaces;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Orders.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class OrderCreationService : IOrderCreationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFoodService _foodService;
        private readonly ICartIdentityAccessor _cartIdentityAccessor;

        public OrderCreationService(
            IUnitOfWork unitOfWork,
            IFoodService foodService,
            ICartIdentityAccessor cartIdentityAccessor)
        {
            _unitOfWork = unitOfWork;
            _foodService = foodService;
            _cartIdentityAccessor = cartIdentityAccessor;
        }

        private string BuildTitleSnapshot(Food food, FoodVariant? variant, IEnumerable<FoodAddon>? addons)
        {
            string baseTitle = variant == null ? food.Name : $"{food.Name} - {variant.Name}";

            if (addons != null)
            {
                var list = addons.ToList();
                if (list.Count > 0)
                {
                    string extra = string.Join(" + ", list.Select(a => a.Name));
                    return $"{baseTitle} ({extra})";
                }
            }

            return baseTitle;
        }

        // Format: {PersianYear}{PersianMonth:D2}{PersianDay:D2}{sequenceForThatRestaurantThatDay}
        // e.g. 140505121 = 1405/05/12, order #1 for that restaurant that day.
        private async Task<string> BuildInvoiceNumberAsync(int restaurantId, DateTime nowUtc, CancellationToken ct)
        {
            var iranOffset = TimeSpan.FromHours(3.5);
            var localNow = nowUtc + iranOffset;
            var pc = new System.Globalization.PersianCalendar();

            var y = pc.GetYear(localNow);
            var m = pc.GetMonth(localNow);
            var d = pc.GetDayOfMonth(localNow);

            var dayStartUtc = pc.ToDateTime(y, m, d, 0, 0, 0, 0) - iranOffset;
            var dayEndUtc = dayStartUtc.AddDays(1);

            var countToday = await _unitOfWork.Order.CountOrdersForRestaurantOnDateAsync(restaurantId, dayStartUtc, dayEndUtc, ct);
            return $"{y:D4}{m:D2}{d:D2}{countToday + 1}";
        }

        /* ============================================================
           Legacy path: create order directly from a client-submitted DTO.
        ============================================================ */
        public async Task<int> CreateOrderAsync(string? userId, CreateOrderDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Items == null || dto.Items.Count == 0)
                throw new Exception("Order must contain at least one item.");

            var order = new Domain.Entities.Order
            {
                RestaurantId = dto.RestaurantId,
                TableLabel = dto.TableLabel,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            order.RestaurantOrderNumber = await _unitOfWork.Order.GetNextRestaurantOrderNumberAsync(dto.RestaurantId);

            if (!string.IsNullOrWhiteSpace(userId))
                order.UserId = userId;

            int totalPrice = 0;

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new Exception("Item quantity must be at least 1.");

                if (!item.FoodId.HasValue)
                    throw new Exception("FoodId is required for each order item.");

                var food = await _unitOfWork.Food.GetFoodWithVariantsAsync(item.FoodId.Value)
                    ?? throw new Exception("Food not found.");

                FoodVariant? variant = null;
                if (item.VariantId.HasValue)
                {
                    variant = food.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value)
                        ?? throw new Exception("Food variant not found.");
                }

                List<FoodAddon> selectedAddons = new();
                var extraIds = item.ExtraIds ?? new List<int>();

                if (extraIds.Count > 0 && variant != null)
                    selectedAddons = variant.Addons.Where(a => extraIds.Contains(a.Id)).ToList();

                int basePrice = variant?.Price ?? food.Price;
                int addonsTotal = selectedAddons.Sum(a => a.ExtraPrice);
                int serverUnitPrice = basePrice + addonsTotal;

                if (serverUnitPrice != item.UnitPrice)
                    throw new Exception("Price mismatch detected. Please refresh and try again.");

                int lineTotal = serverUnitPrice * item.Quantity;
                totalPrice += lineTotal;

                var orderItem = new OrderItem
                {
                    FoodId = food.Id,
                    FoodVariantId = variant?.Id,
                    Quantity = item.Quantity,
                    UnitPrice = serverUnitPrice,
                    TitleSnapshot = BuildTitleSnapshot(food, variant, selectedAddons),
                    VariantTitleSnapshot = variant?.Name,
                    ImageUrlSnapshot = food.ImageUrl,
                    Extras = new List<OrderItemExtra>()
                };

                foreach (var addon in selectedAddons)
                {
                    orderItem.Extras.Add(new OrderItemExtra
                    {
                        FoodAddonId = addon.Id,
                        AddonTitleSnapshot = addon.Name,
                        ExtraPrice = addon.ExtraPrice,
                        Quantity = 1
                    });
                }

                order.OrderItems.Add(orderItem);
            }

            order.TotalPrice = totalPrice;
            order.InvoiceNumber = await BuildInvoiceNumberAsync(dto.RestaurantId, DateTime.UtcNow, CancellationToken.None);

            await _unitOfWork.Order.AddOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(userId))
                _unitOfWork.Order.InvalidateUserRecentOrders(userId);

            return order.Id;
        }

        /* ============================================================
           Main path: checkout the live server-side Cart into an Order.
        ============================================================ */
        public async Task<CheckoutResultDto> CheckoutFromCartAsync(CheckoutRequestDto dto, CancellationToken ct = default)
        {
            var userId = _cartIdentityAccessor.UserId;
            var guestToken = string.IsNullOrWhiteSpace(userId) ? _cartIdentityAccessor.GuestToken : null;

            var cart = await _unitOfWork.Cart.GetActiveCartAsync(userId, guestToken, ct);

            if (cart == null || cart.Items.Count == 0)
                throw new Exception("سبد خرید شما خالی است.");

            if (cart.UpdatedAt < DateTime.UtcNow - TimeSpan.FromHours(2))
            {
                await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                throw new Exception("سبد خرید شما منقضی شده است، لطفا دوباره تلاش کنید.");
            }

            var order = new Domain.Entities.Order
            {
                RestaurantId = cart.RestaurantId,
                TableLabel = dto.TableLabel,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };
            order.RestaurantOrderNumber = await _unitOfWork.Order.GetNextRestaurantOrderNumberAsync(cart.RestaurantId, ct);

            if (!string.IsNullOrWhiteSpace(userId))
                order.UserId = userId;

            int totalPrice = 0;

            // Built inside the loop, where `food`/`variant`/`selectedAddons`
            // are locally in scope — do NOT rebuild this from order.OrderItems
            // afterward, since oi.Food is an unloaded EF navigation and would
            // be null at that point.
            var resultItems = new List<CheckoutResultItemDto>();

            foreach (var cartItem in cart.Items)
            {
                var food = await _unitOfWork.Food.GetFoodWithVariantsAsync(cartItem.FoodId)
                    ?? throw new Exception("یکی از غذاهای سبد خرید دیگر موجود نیست.");

                var variant = food.Variants.FirstOrDefault(v => v.Id == cartItem.FoodVariantId)
                    ?? throw new Exception("یکی از انواع غذای سبد خرید دیگر موجود نیست.");

                var extras = new List<OrderItemExtra>();
                var selectedAddons = new List<FoodAddon>();
                int addonsTotal = 0;

                foreach (var e in cartItem.Extras)
                {
                    var addon = variant.Addons.FirstOrDefault(a => a.Id == e.FoodAddonId);
                    if (addon == null) continue;

                    selectedAddons.Add(addon);
                    addonsTotal += addon.ExtraPrice * e.Quantity;

                    extras.Add(new OrderItemExtra
                    {
                        FoodAddonId = addon.Id,
                        AddonTitleSnapshot = addon.Name,
                        ExtraPrice = addon.ExtraPrice,
                        Quantity = e.Quantity
                    });
                }

                int unitPrice = variant.Price + addonsTotal;
                totalPrice += unitPrice * cartItem.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    FoodId = food.Id,
                    FoodVariantId = variant.Id,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TitleSnapshot = BuildTitleSnapshot(food, variant, selectedAddons),
                    VariantTitleSnapshot = variant.Name,
                    ImageUrlSnapshot = food.ImageUrl,
                    Extras = extras
                });

                resultItems.Add(new CheckoutResultItemDto
                {
                    FoodName = food.Name,
                    VariantName = variant.Name,
                    HasAddons = selectedAddons.Any(),
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                });
            }

            order.TotalPrice = totalPrice;
            order.InvoiceNumber = await BuildInvoiceNumberAsync(cart.RestaurantId, DateTime.UtcNow, ct);

            var paymentMethod = cart.Restaurant.PaymentMethod.ToString();
            var restaurantName = cart.Restaurant.Name;

            await _unitOfWork.Order.AddOrderAsync(order, ct);
            await _unitOfWork.Order.SaveChangesAsync(ct);

            await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
            await _unitOfWork.Cart.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(userId))
                _unitOfWork.Order.InvalidateUserRecentOrders(userId);

            return new CheckoutResultDto
            {
                OrderId = order.Id,
                RestaurantOrderNumber = order.RestaurantOrderNumber,
                InvoiceNumber = order.InvoiceNumber,
                RestaurantName = restaurantName,
                PaymentMethod = paymentMethod,
                TotalPrice = order.TotalPrice,
                TableLabel = order.TableLabel,
                Items = resultItems
            };
        }
    }
}