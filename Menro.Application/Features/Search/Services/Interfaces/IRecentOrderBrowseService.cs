using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.Search.DTOs;

namespace Menro.Application.Features.Search.Services.Interfaces
{
    public interface IRecentOrderBrowseService
    {
        Task<PagedResultDto<RecentOrdersFoodCardDto>> BrowseRecentOrderedFoodsAsync(
            string userId,
            int take = 6,
            string? cursor = null,
            CancellationToken ct = default
        );
    }
}