namespace Menro.Application.Features.Orders.DTOs
{

    public class CreateOrderDto
    {
        public int RestaurantId { get; set; }
        public int? TableNumber { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
