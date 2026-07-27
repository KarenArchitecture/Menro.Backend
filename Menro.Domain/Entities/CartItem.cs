namespace Menro.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public int FoodVariantId { get; set; }
        public FoodVariant FoodVariant { get; set; } = null!;

        public int Quantity { get; set; }

        public ICollection<CartItemExtra> Extras { get; set; } = new List<CartItemExtra>();
    }
}