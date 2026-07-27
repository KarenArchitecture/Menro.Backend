using Menro.Application.Features.Orders.DTOs;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IOrderHistoryService
    {
        Task<List<UserOrderListItemDto>> GetUserOrdersAsync(string userId);
        Task<PublicOrderDetailsDto?> GetOrderBillAsync(int orderId);
    }
}