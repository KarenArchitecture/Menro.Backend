using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private readonly MenroDbContext _context;

        public CartRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cart?> GetActiveCartAsync(string? userId, string? guestToken, CancellationToken ct = default)
        {
            IQueryable<Cart> query = _context.Carts
                .Include(c => c.Restaurant)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Extras)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Food)
                .Include(c => c.Items)
                    .ThenInclude(i => i.FoodVariant);

            if (!string.IsNullOrWhiteSpace(userId))
                return await query.FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (!string.IsNullOrWhiteSpace(guestToken))
                return await query.FirstOrDefaultAsync(c => c.GuestToken == guestToken, ct);

            return null;
        }

        public async Task AddCartAsync(Cart cart, CancellationToken ct = default)
            => await _context.Carts.AddAsync(cart, ct);

        public Task RemoveCartItemAsync(CartItem item, CancellationToken ct = default)
        {
            _context.CartItems.Remove(item);
            return Task.CompletedTask;
        }

        public Task RemoveCartAsync(Cart cart, CancellationToken ct = default)
        {
            _context.Carts.Remove(cart);
            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct) > 0;

        public async Task<List<Cart>> GetExpiredCartsAsync(DateTime threshold, CancellationToken ct = default)
        {
            return await _context.Carts
                .Where(c => c.UpdatedAt < threshold)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Extras)
                .ToListAsync(ct);
        }
    }
}