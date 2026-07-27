namespace Menro.Domain.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        // Exactly one of these is set: UserId for logged-in users, GuestToken for guests.
        public string? UserId { get; set; }
        public User? User { get; set; }
        public string? GuestToken { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Used for the 2-hour inactivity expiry.
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}