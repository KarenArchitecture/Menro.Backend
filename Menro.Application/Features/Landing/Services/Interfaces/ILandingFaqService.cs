using Menro.Application.Features.Landing.DTOs;

namespace Menro.Application.Features.Landing.Services.Interfaces
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
