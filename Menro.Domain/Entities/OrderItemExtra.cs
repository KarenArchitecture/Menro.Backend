namespace Menro.Domain.Entities
{
    public class OrderItemExtra
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        public int? FoodAddonId { get; set; }
        public FoodAddon? FoodAddon { get; set; }

        public string AddonTitleSnapshot { get; set; } = string.Empty;

        // Unit price of a single addon — int, same as FoodAddon.ExtraPrice.
        public int ExtraPrice { get; set; }

        // How many of this addon were selected for this order line.
        public int Quantity { get; set; } = 1;
    }
}