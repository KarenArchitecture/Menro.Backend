using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Common.Interfaces
{
    public interface IMediaStorageProvider
    {
        Task<MediaSaveResult> SaveAsync(
            MediaCategory category,
            IFormFile file,
            string? entityId = null,
            string? oldFileName = null,
            CancellationToken ct = default);

        // for byte[] sources (e.g. cover art extracted from an mp3's embedded tags)
        Task<MediaSaveResult> SaveBytesAsync(
            MediaCategory category, byte[] bytes, string extension,
            string? entityId = null, CancellationToken ct = default);

        // deletes every generated variant (original/resized/thumbnail) for this file
        bool Delete(MediaCategory category, string fileName, string? entityId = null);

        string GetUrl(MediaCategory category, string fileName, string? entityId = null, MediaVariant variant = MediaVariant.Original);
        string GetBaseUrl();

        // for non-public categories (e.g. RestaurantMusicFile) that need disk streaming, not a URL
        string GetPhysicalPath(MediaCategory category, string fileName, string? entityId = null, MediaVariant variant = MediaVariant.Original);
    }

    public record MediaSaveResult(string FileName, string Url);
}
