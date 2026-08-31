using Menro.Application.Features.SiteContent.DTOs;

namespace Menro.Application.Features.SiteContent.Services.Interfaces
{
    public interface ILandingReasonService
    {
        Task<List<LandingReasonResponse>> GetAllAsync();

        Task<LandingReasonResponse> CreateAsync(CreateLandingReasonRequest request);

        Task<LandingReasonResponse> UpdateAsync(Guid id, UpdateLandingReasonRequest request);

        Task DeleteAsync(Guid id);

        /// <summary>Swaps this item with its neighbor. direction must be "up" or "down".</summary>
        Task MoveAsync(Guid id, string direction);
    }
}
