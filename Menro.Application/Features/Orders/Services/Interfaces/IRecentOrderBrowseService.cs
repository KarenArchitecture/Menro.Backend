using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Search.DTOs;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IRecentOrderBrowseService
    {
        Task<PagedResultDto<RecentOrdersFoodCardDto>> BrowseRecentOrderedFoodsAsync(
            string userId,
            int take,
            string? cursor,
            CancellationToken ct = default);
    }
}