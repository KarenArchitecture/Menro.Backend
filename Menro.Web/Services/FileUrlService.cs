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

        public string BuildFileUrl(string relativePath)
        {
            return $"{_baseUrl}/{relativePath.TrimStart('/')}";
        }
        /*--------------*/
        /*--- icons  ---*/
        /*--------------*/
        public string BuildIconUrl(string fileName)
        {
            return BuildFileUrl($"icons/{fileName}");
        }

        /*--------------*/
        /*--- images ---*/
        /*--------------*/
        // general
        public string BuildImageUrl(string fileName)
        {
            return BuildFileUrl($"img/{fileName}");
        }
        
        // profile
        public string BuildProfileImageUrl(string fileName)
        {
            return BuildFileUrl($"img/profile/{fileName}");
        }
        
        // ad
        public string BuildAdImageUrl(string fileName)
        {
            return BuildFileUrl($"img/adBanner/{fileName}");
        }

        // food
        public string BuildFoodImageUrl(string fileName)
        {
            return BuildFileUrl($"img/food/{fileName}");
        }
        
        // restaurant
        public string BuildRestaurantHomeBannerUrl(string fileName)
        {
            return BuildFileUrl($"img/restaurants/home/{fileName}");
        }
        public string BuildRestaurantShopBannerUrl(string fileName)
        {
            return BuildFileUrl($"img/restaurants/shop/{fileName}");
        }
        public string BuildRestaurantLogoUrl(string fileName)
        {
            return BuildFileUrl($"img/restaurants/logo/{fileName}");
        }

        /*--------------*/
        /*--- audio  ---*/
        /*--------------*/
        public string BuildAudioUrl(string fileName)
        {
            return BuildFileUrl($"audio/{fileName}");
        }
    }
}
