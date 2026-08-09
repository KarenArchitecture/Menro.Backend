using Menro.Domain.Enums;


namespace Menro.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int RestaurantOrderNumber { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public int? RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? TableLabel { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

}
