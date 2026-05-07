namespace Menro.Application.Features.Orders.DTOs
{
    public class AdminOrderItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } // URL یا null

        public List<AdminOrderItemAddonDto> Addons { get; set; } = new();
    }

    public class AdminOrderItemAddonDto
    {
        public string Name { get; set; } = "";
    }

}
