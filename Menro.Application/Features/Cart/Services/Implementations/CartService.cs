using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Cart.DTOs;
using Menro.Application.Features.Cart.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Cart.Services.Implementations
{
    public class CartService : ICartService
    {
        private static readonly TimeSpan CartLifetime = TimeSpan.FromHours(2);

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartIdentityAccessor _identity;
        private readonly IMediaStorageProvider _mediaStorage;

        public CartService(IUnitOfWork unitOfWork, ICartIdentityAccessor identity, IMediaStorageProvider mediaStorage)
        {
            _unitOfWork = unitOfWork;
            _identity = identity;
            _mediaStorage = mediaStorage;
        }

        private (string? userId, string? guestToken) ResolveIdentity()
        {
            var userId = _identity.UserId;
            var guestToken = string.IsNullOrWhiteSpace(userId) ? _identity.GuestToken : null;

            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(guestToken))
                throw new Exception("Cart identity missing. Guest requests must send the X-Guest-Cart-Id header.");

            return (userId, guestToken);
        }

        private async Task<Domain.Entities.Cart?> GetLiveCartAsync(CancellationToken ct)
        {
            var (userId, guestToken) = ResolveIdentity();
            var cart = await _unitOfWork.Cart.GetActiveCartAsync(userId, guestToken, ct);
            if (cart == null) return null;

            if (cart.UpdatedAt < DateTime.UtcNow - CartLifetime)
            {
                await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                return null;
            }

            return cart;
        }

        public async Task<CartDto> GetCartAsync(CancellationToken ct = default)
        {
            var cart = await GetLiveCartAsync(ct);
            return await MapAsync(cart);
        }

        public async Task<CartOperationResultDto> SetItemAsync(SetCartItemRequestDto dto, CancellationToken ct = default)
        {
            var food = await _unitOfWork.Food.GetFoodWithVariantsAsync(dto.FoodId)
                ?? throw new Exception("غذا یافت نشد.");

            var variant = dto.VariantId.HasValue
                ? food.Variants.FirstOrDefault(v => v.Id == dto.VariantId.Value && !v.IsDeleted && v.IsAvailable)
                : food.Variants.Where(v => !v.IsDeleted && v.IsAvailable)
                    .OrderByDescending(v => v.IsDefault == true)
                    .FirstOrDefault();

            if (variant == null)
                throw new Exception("نوع غذا یافت نشد.");

            var cart = await GetLiveCartAsync(ct);

            if (dto.Quantity > 0 && cart != null && cart.RestaurantId != food.RestaurantId)
            {
                if (!dto.ConfirmRestaurantSwitch)
                {
                    return new CartOperationResultDto
                    {
                        RequiresConfirmation = true,
                        ConflictingRestaurantName = cart.Restaurant?.Name,
                        Cart = await MapAsync(cart)
                    };
                }

                await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                cart = null;
            }

            if (cart == null)
            {
                if (dto.Quantity <= 0)
                    return new CartOperationResultDto { Cart = new CartDto() };

                var (userId, guestToken) = ResolveIdentity();
                cart = new Domain.Entities.Cart
                {
                    UserId = userId,
                    GuestToken = userId == null ? guestToken : null,
                    RestaurantId = food.RestaurantId,
                };
                await _unitOfWork.Cart.AddCartAsync(cart, ct);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.FoodId == food.Id && i.FoodVariantId == variant.Id);

            if (dto.Quantity <= 0)
            {
                if (existingItem != null)
                {
                    cart.Items.Remove(existingItem);
                    await _unitOfWork.Cart.RemoveCartItemAsync(existingItem, ct);
                }
            }
            else
            {
                var validAddonIds = variant.Addons.Where(a => !a.IsDeleted).Select(a => a.Id).ToHashSet();
                var extras = (dto.Addons ?? new())
                    .Where(a => a.Quantity > 0 && validAddonIds.Contains(a.FoodAddonId))
                    .Select(a => new CartItemExtra { FoodAddonId = a.FoodAddonId, Quantity = a.Quantity })
                    .ToList();

                if (existingItem != null)
                {
                    existingItem.Quantity = dto.Quantity;
                    existingItem.Extras.Clear();
                    foreach (var e in extras) existingItem.Extras.Add(e);
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        FoodId = food.Id,
                        FoodVariantId = variant.Id,
                        Quantity = dto.Quantity,
                        Extras = extras
                    });
                }
            }

            if (cart.Items.Count == 0)
            {
                await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                return new CartOperationResultDto { Cart = new CartDto() };
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Cart.SaveChangesAsync(ct);

            return new CartOperationResultDto { Cart = await MapAsync(cart) };
        }

        public async Task ClearCartAsync(CancellationToken ct = default)
        {
            var cart = await GetLiveCartAsync(ct);
            if (cart == null) return;

            await _unitOfWork.Cart.RemoveCartAsync(cart, ct);
            await _unitOfWork.Cart.SaveChangesAsync(ct);
        }

        // All prices are int (Toman has no fractional unit). Prices are always
        // recomputed live from Food/Variant/Addon — never trusted from storage.
        private async Task<CartDto> MapAsync(Domain.Entities.Cart? cart)
        {
            if (cart == null || cart.Items.Count == 0)
                return new CartDto();

            var dto = new CartDto
            {
                Id = cart.Id,
                RestaurantId = cart.RestaurantId,
                RestaurantName = cart.Restaurant?.Name,
                RestaurantSlug = cart.Restaurant?.Slug,
                TableCount = cart.Restaurant?.TableCount ?? 0,
                ExpiresAt = cart.UpdatedAt + CartLifetime
            };

            dto.PaymentMethod = cart.Restaurant?.PaymentMethod.ToString() ?? "";

            foreach (var item in cart.Items)
            {
                var food = await _unitOfWork.Food.GetFoodWithVariantsAsync(item.FoodId);
                var variant = food?.Variants.FirstOrDefault(v => v.Id == item.FoodVariantId);
                if (food == null || variant == null) continue;

                var addonDtos = new List<CartItemAddonDto>();
                int addonsTotal = 0;

                foreach (var extra in item.Extras)
                {
                    var addon = variant.Addons.FirstOrDefault(a => a.Id == extra.FoodAddonId);
                    if (addon == null) continue;

                    addonDtos.Add(new CartItemAddonDto
                    {
                        FoodAddonId = addon.Id,
                        Name = addon.Name,
                        ExtraPrice = addon.ExtraPrice,
                        Quantity = extra.Quantity
                    });
                    addonsTotal += addon.ExtraPrice * extra.Quantity;
                }

                int unitPrice = variant.Price + addonsTotal;
                int lineTotal = unitPrice * item.Quantity;
                var availableAddonDtos = variant.Addons
                    .Where(a => !a.IsDeleted)
                    .Select(a => new CartItemAddonDto
                    {
                        FoodAddonId = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice,
                        Quantity = addonDtos.FirstOrDefault(sel => sel.FoodAddonId == a.Id)?.Quantity ?? 0
                    })
                    .ToList();

                dto.Items.Add(new CartItemDto
                {
                    Id = item.Id,
                    FoodId = food.Id,
                    FoodName = food.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(food.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, food.ImageUrl, food.Id.ToString(), MediaVariant.Resized),
                    VariantId = variant.Id,
                    VariantName = variant.Name,
                    IsDefaultVariant = variant.IsDefault == true,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal,
                    Addons = addonDtos,
                    AvailableAddons = availableAddonDtos,
                    Rating = food.AverageRating,
                    Voters = food.VotersCount,
                });

                dto.Total += lineTotal;
                dto.Count += item.Quantity;
            }

            return dto;
        }

        public async Task<CartDto> MergeGuestCartAsync(CancellationToken ct = default)
        {
            var userId = _identity.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("کاربر لاگین نیست.");

            var guestToken = _identity.GuestToken;
            if (string.IsNullOrWhiteSpace(guestToken))
                return await GetCartAsync(ct);

            var guestCart = await _unitOfWork.Cart.GetActiveCartAsync(null, guestToken, ct);
            if (guestCart == null || guestCart.Items.Count == 0)
                return await GetCartAsync(ct);

            var userCart = await _unitOfWork.Cart.GetActiveCartAsync(userId, null, ct);

            if (userCart == null)
            {
                guestCart.UserId = userId;
                guestCart.GuestToken = null;
                guestCart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                return await MapAsync(guestCart);
            }

            if (userCart.RestaurantId == guestCart.RestaurantId)
            {
                foreach (var gi in guestCart.Items)
                {
                    var existing = userCart.Items.FirstOrDefault(i => i.FoodId == gi.FoodId && i.FoodVariantId == gi.FoodVariantId);
                    if (existing != null)
                    {
                        existing.Quantity += gi.Quantity;
                        foreach (var ge in gi.Extras)
                        {
                            var existingExtra = existing.Extras.FirstOrDefault(e => e.FoodAddonId == ge.FoodAddonId);
                            if (existingExtra != null)
                                existingExtra.Quantity = Math.Max(existingExtra.Quantity, ge.Quantity);
                            else
                                existing.Extras.Add(new CartItemExtra { FoodAddonId = ge.FoodAddonId, Quantity = ge.Quantity });
                        }
                    }
                    else
                    {
                        userCart.Items.Add(new CartItem
                        {
                            FoodId = gi.FoodId,
                            FoodVariantId = gi.FoodVariantId,
                            Quantity = gi.Quantity,
                            Extras = gi.Extras.Select(e => new CartItemExtra { FoodAddonId = e.FoodAddonId, Quantity = e.Quantity }).ToList()
                        });
                    }
                }
                userCart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Cart.RemoveCartAsync(guestCart, ct);
                await _unitOfWork.Cart.SaveChangesAsync(ct);
                return await MapAsync(userCart);
            }

            // Different restaurants: keep the user's own cart, drop the guest one.
            await _unitOfWork.Cart.RemoveCartAsync(guestCart, ct);
            await _unitOfWork.Cart.SaveChangesAsync(ct);
            return await MapAsync(userCart);
        }
    }
}