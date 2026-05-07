using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Orders.DTOs
{
    public class CreateOrderItemDto
    {
        public int? FoodId { get; set; }
        public int? VariantId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        public List<int> ExtraIds { get; set; } = new();
    }
}
