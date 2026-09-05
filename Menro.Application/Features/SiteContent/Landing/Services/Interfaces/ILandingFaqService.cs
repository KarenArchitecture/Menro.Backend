using Menro.Application.Features.SiteContent.DTOs;

namespace Menro.Application.Features.SiteContent.Services.Interfaces
{
    public interface ILandingFaqService
    {
        Task<List<LandingFaqResponse>> GetAllAsync();

        Task<LandingFaqResponse> CreateAsync(CreateLandingFaqRequest request);

        Task<LandingFaqResponse> UpdateAsync(Guid id, UpdateLandingFaqRequest request);

        Task DeleteAsync(Guid id);

        /// <summary>Swaps this item with its neighbor. direction must be "up" or "down".</summary>
        Task MoveAsync(Guid id, string direction);
    }
}
