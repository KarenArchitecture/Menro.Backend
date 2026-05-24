using Microsoft.AspNetCore.Http;

namespace Menro.Application.Common.Interfaces
{
    public interface IFileService
    {
        /* ICONS */
        Task<string> UploadSvgAsync(IFormFile file);
        bool DeleteIcon(string fileName);

        /* PROFILE */
        Task<string> UploadProfileImageAsync(
            IFormFile file,
            string? oldFileName = null);

        /* ADS */
        Task<string> UploadAdImageAsync(IFormFile file);

        Task<string> UploadCarouselImageAsync(IFormFile file);

        /* FOOD */
        Task<string> UploadFoodImageAsync(IFormFile file);
        bool DeleteFoodImage(string fileName);

        /* RESTAURANTS */
        Task<string> UploadRestaurantHomeBannerAsync(
            IFormFile file,
            string? oldFileName = null);

        Task<string> UploadRestaurantShopBannerAsync(
            IFormFile file,
            string? oldFileName = null);

        Task<string> UploadRestaurantLogoAsync(
            IFormFile file,
            string? oldFileName = null);

        /* MUSIC */
        Task<string> UploadMusicAsync(IFormFile file);

        Task<string> UploadMusicCoverAsync(
            IFormFile file,
            string? oldFileName = null);
    }
}