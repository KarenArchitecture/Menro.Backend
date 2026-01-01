namespace Menro.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public int? FoodVariantId { get; set; }
        public FoodVariant? FoodVariant { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string TitleSnapshot { get; set; } = string.Empty;

        // ✅ اگر Variant انتخاب شده بود، اسمش هم ثابت بماند
        public string? VariantTitleSnapshot { get; set; }

        public ICollection<OrderItemExtra> Extras { get; set; } = new List<OrderItemExtra>();
    }
}
