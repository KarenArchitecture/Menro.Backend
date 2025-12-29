using Menro.Application.Features.Orders.DTOs;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IOrderCreationService
    {
        Task<int> CreateOrderAsync(string? userId, CreateOrderDto dto);
    }
}
