using Menro.Domain.Enums;

namespace Menro.Application.Features.Orders.DTOs
{
    public class CheckoutRequestDto
    {
        public int? TableNumber { get; set; }
    }

    public class CheckoutResultDto
    {
        public int OrderId { get; set; }
        public string SuccessType { get; set; } = "checkout";
        public string? InvoiceNumber { get; set; }
    }

    public class UserOrderListItemDto
    {
        public int Id { get; set; }
        public int RestaurantOrderNumber { get; set; }
        public string RestaurantName { get; set; } = "";
        public string? RestaurantLogoUrl { get; set; }
        public int? TableNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public List<UserOrderPreviewItemDto> PreviewItems { get; set; } = new();
    }

    public class UserOrderPreviewItemDto
    {
        public int FoodId { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
    }

    public class PublicOrderDetailsDto
    {
        public int Id { get; set; }
        public int RestaurantOrderNumber { get; set; }
        public string RestaurantName { get; set; } = "";
        public int? TableNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public List<PublicOrderItemDto> Items { get; set; } = new();
    }

    public class PublicOrderItemDto
    {
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public List<PublicOrderAddonDto> Addons { get; set; } = new();
    }

    public class PublicOrderAddonDto
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public int ExtraPrice { get; set; }
    }
}