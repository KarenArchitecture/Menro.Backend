using Menro.Application.Features.Landing.DTOs;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Landing.Services.Interfaces
{
    public interface ILandingGeneralService
    {
        Task<LandingGeneralResponse> GetAsync();

        Task<LandingGeneralResponse> UpdateAsync(UpdateLandingGeneralRequest request);

        /// <summary>
        /// Uploads the new hero image and - if <paramref name="oldFileName"/> is
        /// provided - deletes the previous one, same convention as
        /// BlogPostsController's cover image upload.
        /// </summary>
        Task<UploadLandingHeroImageResponse> UploadHeroImageAsync(IFormFile file, string? oldFileName);
    }
}
