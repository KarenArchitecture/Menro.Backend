namespace Menro.Application.Features.Cart.DTOs
{
    public class CartDto
    {
        public int? Id { get; set; }
        public int? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public string? RestaurantSlug { get; set; }
        public int TableCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Count { get; set; }
        public string PaymentMethod { get; set; } = "";
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = "";
        public bool IsDefaultVariant { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public int LineTotal { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public List<CartItemAddonDto> Addons { get; set; } = new();
        public List<CartItemAddonDto> AvailableAddons { get; set; } = new();
    }

    public class CartItemAddonDto
    {
        public int FoodAddonId { get; set; }
        public string Name { get; set; } = "";
        public int ExtraPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class AddonSelectionDto
    {
        public int FoodAddonId { get; set; }
        public int Quantity { get; set; }
    }

    public class SetCartItemRequestDto
    {
        public int FoodId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
        public List<AddonSelectionDto> Addons { get; set; } = new();
        public bool ConfirmRestaurantSwitch { get; set; } = false;
    }

    public class CartOperationResultDto
    {
        public bool RequiresConfirmation { get; set; }
        public string? ConflictingRestaurantName { get; set; }
        public CartDto? Cart { get; set; }
    }
}