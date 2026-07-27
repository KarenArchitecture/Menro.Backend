using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetActiveCartAsync(string? userId, string? guestToken, CancellationToken ct = default);
        Task AddCartAsync(Cart cart, CancellationToken ct = default);
        Task RemoveCartItemAsync(CartItem item, CancellationToken ct = default);
        Task RemoveCartAsync(Cart cart, CancellationToken ct = default);
        Task<bool> SaveChangesAsync(CancellationToken ct = default);
        Task<List<Cart>> GetExpiredCartsAsync(DateTime threshold, CancellationToken ct = default);
    }
}