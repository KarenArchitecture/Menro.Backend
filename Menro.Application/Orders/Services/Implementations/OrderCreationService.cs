using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;
using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Orders.Services.Implementations
{
    /// <summary>
    /// Implements the main checkout order creation flow.
    /// Maps CreateOrderDto → Order + OrderItems + OrderItemExtras.
    /// Supports both logged-in users (userId != null)
    /// and guest users (userId == null).
    /// </summary>
    public class OrderCreationService : IOrderCreationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFoodService _foodService;

        public OrderCreationService(IUnitOfWork unitOfWork,
            IFoodService foodService)
        {
            _unitOfWork = unitOfWork;
            _foodService = foodService;

        }

        /* ============================================================
           Helper: build snapshot title for history display
           (e.g. "کباب - بزرگ (سس تند + نوشابه)")
        ============================================================ */
        private string BuildTitleSnapshot(Food food, FoodVariant? variant, IEnumerable<FoodAddon>? addons)
        {
            string baseTitle = variant == null
                ? food.Name
                : $"{food.Name} - {variant.Name}";

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

        /* ============================================================
           Main: Create Order
        ============================================================ */
        public async Task<int> CreateOrderAsync(string? userId, CreateOrderDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Items == null || dto.Items.Count == 0)
                throw new Exception("Order must contain at least one item.");

            // 🔹 IMPORTANT CHANGE:
            // We NO LONGER require userId here.
            // If userId is null => guest order.

            /* -------------------------------
               Create root Order entity
            --------------------------------*/
            var order = new Order
            {
                RestaurantId = dto.RestaurantId,
                TableCode = dto.TableCode,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            // 🔹 Attach user only if logged-in
            if (!string.IsNullOrWhiteSpace(userId))
            {
                order.UserId = userId;
            }

            decimal totalAmount = 0;

            /* -------------------------------
               Process each order item
            --------------------------------*/
            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new Exception("Item quantity must be at least 1.");

                // We expect FoodId to be present for ALL items.
                // For variant items, frontend should send BOTH FoodId and VariantId.
                if (!item.FoodId.HasValue)
                    throw new Exception("FoodId is required for each order item.");

                //var food = await _unitOfWork.Food.GetFoodDetailsAsync(item.FoodId.Value);
                var food = _foodService.GetFoodDetailsAsync(item.FoodId.Value, dto.RestaurantId);
                if (food == null)
                    throw new Exception("Food not found.");

                /* ---------------- Variant ---------------- */
                FoodVariant? variant = null;
                if (item.VariantId.HasValue)
                {
                    variant = food.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value);
                    if (variant == null)
                        throw new Exception("Food variant not found.");
                }

                /* ---------------- Addons ----------------- */
                List<FoodAddon> selectedAddons = new();
                var extraIds = item.ExtraIds ?? new List<int>();

                if (extraIds.Count > 0 && variant != null)
                {
                    // addons belong to this variant
                    selectedAddons = variant.Addons
                        .Where(a => extraIds.Contains(a.Id))
                        .ToList();
                }

                /* ---------------- Server-side pricing ---------------- */
                decimal basePrice = variant?.Price ?? food.Price;
                decimal addonsTotal = selectedAddons.Sum(a => a.ExtraPrice);
                decimal serverUnitPrice = basePrice + addonsTotal;

                // Protect against frontend tampering
                if (serverUnitPrice != item.UnitPrice)
                    throw new Exception("Price mismatch detected. Please refresh and try again.");

                decimal lineTotal = serverUnitPrice * item.Quantity;
                totalAmount += lineTotal;

                /* ---------------- Build OrderItem ---------------- */
                var orderItem = new OrderItem
                {
                    FoodId = food.Id,
                    Food = food,
                    FoodVariantId = variant?.Id,
                    FoodVariant = variant,
                    Quantity = item.Quantity,
                    UnitPrice = serverUnitPrice,
                    TitleSnapshot = BuildTitleSnapshot(food, variant, selectedAddons),
                    Extras = new List<OrderItemExtra>()
                };

                // Map selected addons → OrderItemExtra
                foreach (var addon in selectedAddons)
                {
                    orderItem.Extras.Add(new OrderItemExtra
                    {
                        FoodAddonId = addon.Id,
                        FoodAddon = addon,
                        ExtraPrice = addon.ExtraPrice
                    });
                }

                order.OrderItems.Add(orderItem);
            }

            /* ---------------- Finalize total ---------------- */
            order.TotalAmount = totalAmount;

            /* ---------------- Save Order ---------------- */
            await _unitOfWork.Order.AddOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // 🔹 Invalidate recent-orders cache only for logged-in users
            if (!string.IsNullOrWhiteSpace(userId))
            {
                _unitOfWork.Order.InvalidateUserRecentOrders(userId);
            }

            return order.Id;
        }
    }
}
