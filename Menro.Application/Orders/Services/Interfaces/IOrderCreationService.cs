using System.Threading.Tasks;
using Menro.Application.Orders.DTOs;

namespace Menro.Application.Orders.Services.Interfaces
{
    /// <summary>
    /// Creates orders from the checkout flow:
    /// validates restaurant, foods, variants, addons and pricing,
    /// then persists the Order graph.
    /// </summary>
    public interface IOrderCreationService
    {
        /// <summary>
        /// Creates a new Order for the given user (or guest).
        /// If userId is null, the order is stored as a guest order.
        /// Returns the created Order.Id.
        /// </summary>
        Task<int> CreateOrderAsync(string? userId, CreateOrderDto dto);
    }
}
