using Menro.Domain.Enums;

namespace Menro.Application.Features.Orders.DTOs
{
    public class AdminOrderListItemDto
    {
        public long Id { get; set; }

        public int RestaurantOrderNumber { get; set; }
        public string InvoiceNumber { get; set; } = "";


        public string PaymentMethod { get; set; } = "";

        public int? TableNumber { get; set; }

        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
