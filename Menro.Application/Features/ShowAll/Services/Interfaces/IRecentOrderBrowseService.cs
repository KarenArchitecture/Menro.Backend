using System.Threading;
using System.Threading.Tasks;
using Menro.Application.Features.Orders.DTOs;
using Menro.Application.Features.ShowAll.DTOs;

namespace Menro.Application.Features.ShowAll.Services.Interfaces
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