namespace Menro.Application.Features.Orders.DTOs
{
    public class OrderCreatedNotification
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}