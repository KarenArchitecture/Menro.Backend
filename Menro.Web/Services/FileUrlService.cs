using Menro.Application.Common.Interfaces;

namespace Menro.Web.Services.Implementations
{
    public class FileUrlService : IFileUrlService
    {
        private readonly string _baseUrl;

        public FileUrlService(IConfiguration config)
        {
            _baseUrl = config["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";
        }

        private string Clean(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            return path
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/')
                .TrimEnd();
        }

        private string Normalize(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "";

            var cleaned = Clean(fileName);

            // remove accidental img/ prefix
            if (cleaned.StartsWith("img/"))
                cleaned = cleaned["img/".Length..];

            return cleaned;
        }

        public string BuildFileUrl(string relativePath)
        {
            var cleanPath = Clean(relativePath);

            if (string.IsNullOrWhiteSpace(cleanPath))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(_baseUrl))
                return "/" + cleanPath;

            return $"{_baseUrl}/{cleanPath}";
        }


        /* ICONS */
        public string BuildIconUrl(string fileName)
            => BuildFileUrl($"icons/{Normalize(fileName)}");

        /* GENERAL IMAGES */
        public string BuildImageUrl(string fileName)
            => BuildFileUrl($"img/{Normalize(fileName)}");

        /* PROFILE */
        public string BuildProfileImageUrl(string fileName)
            => BuildFileUrl($"img/profile/{Normalize(fileName)}");

        /* ADS */
        public string BuildAdImageUrl(string fileName)
            => BuildFileUrl($"img/ads/banner/{Normalize(fileName)}");

        public string BuildCarouselImageUrl(string fileName)
            => BuildFileUrl($"img/ads/carousel/{Normalize(fileName)}");

        /* FOOD */
        public string BuildFoodImageUrl(string fileName)
            => BuildFileUrl($"img/food/{Normalize(fileName)}");

        /* RESTAURANTS */
        public string BuildRestaurantHomeBannerUrl(string fileName)
            => BuildFileUrl($"img/restaurant/home/{Normalize(fileName)}");

        public string BuildRestaurantShopBannerUrl(string fileName)
            => BuildFileUrl($"img/restaurant/shop/{Normalize(fileName)}");

        public string BuildRestaurantLogoUrl(string fileName)
            => BuildFileUrl($"img/restaurant/logo/{Normalize(fileName)}");

        /* MUSIC */
        /* MUSIC FILE URL */
        public string BuildMusicFileUrl(string fileName)
            => BuildFileUrl($"media/music/files/{Normalize(fileName)}");
        public string BuildMusicCoverUrl(string fileName)
            => BuildFileUrl($"media/music/covers/{Normalize(fileName)}");

    }
}