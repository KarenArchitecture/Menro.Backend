namespace Menro.Domain.Entities
{
    public class CartItemExtra
    {
        public int Id { get; set; }

        public int CartItemId { get; set; }
        public CartItem CartItem { get; set; } = null!;

        public int FoodAddonId { get; set; }
        public FoodAddon FoodAddon { get; set; } = null!;

        // e.g. "×3 extra cheese"
        public int Quantity { get; set; } = 1;
    }
}