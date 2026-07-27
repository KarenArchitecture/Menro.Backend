using Menro.Application.Features.Cart.DTOs;

namespace Menro.Application.Features.Cart.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(CancellationToken ct = default);
        Task<CartOperationResultDto> SetItemAsync(SetCartItemRequestDto dto, CancellationToken ct = default);
        Task ClearCartAsync(CancellationToken ct = default);
        Task<CartDto> MergeGuestCartAsync(CancellationToken ct = default);
    }
}