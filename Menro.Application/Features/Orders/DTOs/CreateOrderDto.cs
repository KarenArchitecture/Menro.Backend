namespace Menro.Application.Features.Orders.DTOs
{

    public class CreateOrderDto
    {
        public int RestaurantId { get; set; }
        public string? TableLabel { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
