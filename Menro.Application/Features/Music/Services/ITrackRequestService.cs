using Menro.Application.Features.Music.DTOs.Requests;

namespace Menro.Application.Features.Music.Services
{
    public interface ITrackRequestService
    {
        Task<List<RequestedTrackDto>> GetPendingAsync(int restaurantId);
        Task<bool> RejectAsync(Guid requestId, int restaurantId);
        Task<bool> ApproveAsync(Guid requestId, int restaurantId);
    }
}
