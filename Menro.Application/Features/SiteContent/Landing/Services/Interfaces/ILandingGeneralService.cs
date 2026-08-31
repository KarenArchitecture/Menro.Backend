using Menro.Application.Features.SiteContent.DTOs;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.SiteContent.Services.Interfaces
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
    }
}
