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
        // icons
        public string BuildIconUrl(string fileName)
        {
            return BuildFileUrl($"icons/{fileName}");
        }
        // images
        public string BuildImageUrl(string fileName)
        {
            return BuildFileUrl($"img/{fileName}");
        }
        public string BuildProfileImageUrl(string fileName)
        {
            return BuildFileUrl($"img/profile/{fileName}");
        }
        // audios
        public string BuildAudioUrl(string fileName)
        {
            return BuildFileUrl($"audio/{fileName}");
        }
    }
}
