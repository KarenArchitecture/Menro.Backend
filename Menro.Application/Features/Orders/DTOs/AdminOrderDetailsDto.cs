using Menro.Domain.Enums;

namespace Menro.Application.Features.Orders.DTOs
{
    public class AdminOrderDetailsDto
    {
        public long Id { get; set; }
        public int RestaurantOrderNumber { get; set; }
        public int? TableNumber { get; set; }

        public OrderStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public decimal TotalPrice { get; set; }

        public List<AdminOrderItemDto> Items { get; set; } = new();
    }
}
