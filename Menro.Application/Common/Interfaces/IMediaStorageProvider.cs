using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Common.Interfaces
{
    public interface IMediaStorageProvider
    {
        Task<MediaSaveResult> SaveAsync(
            MediaCategory category,
            IFormFile file,
            string? ownerId = null,
            string? oldFileName = null,
            CancellationToken ct = default);

        // for byte[] sources (e.g. cover art extracted from an mp3's embedded tags)
        Task<MediaSaveResult> SaveBytesAsync(
            MediaCategory category, byte[] bytes, string extension,
            string? ownerId = null, CancellationToken ct = default);

        bool Delete(MediaCategory category, string fileName, string? ownerId = null);

        string GetUrl(MediaCategory category, string fileName, string? ownerId = null);
        string GetBaseUrl();

        // for non-public categories (e.g. RestaurantMusicFile) that need disk streaming, not a URL
        string GetPhysicalPath(MediaCategory category, string fileName, string? ownerId = null);

    }

    public record MediaSaveResult(string FileName, string Url);
}
