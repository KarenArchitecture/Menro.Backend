namespace Menro.Application.Common.Interfaces
{
    public interface IFileUrlService
    {
        string BuildFileUrl(string relativePath);
        string BuildIconUrl(string fileName);
        string BuildImageUrl(string fileName);
        string BuildProfileImageUrl(string fileName);
        string BuildAdImageUrl(string fileName);
        string BuildFoodImageUrl(string fileName);
        string BuildRestaurantHomeBannerUrl(string fileName);
        string BuildRestaurantShopBannerUrl(string fileName);
        string BuildRestaurantLogoUrl(string fileName);

        string BuildAudioUrl(string fileName);
    }
}
