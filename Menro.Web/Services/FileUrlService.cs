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

        public string BuildIconUrl(string fileName)
        {
            return BuildFileUrl($"icons/{fileName}");
        }

        public string BuildImageUrl(string fileName)
        {
            return BuildFileUrl($"images/{fileName}");
        }
        public string BuildProfileImageUrl(string fileName)
        {
            return BuildFileUrl($"images/profile/{fileName}");
        }

        public string BuildAudioUrl(string fileName)
        {
            return BuildFileUrl($"audio/{fileName}");
        }
    }
}
