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

        bool Delete(MediaCategory category, string fileName, string? ownerId = null);

        string GetUrl(MediaCategory category, string fileName, string? ownerId = null);
    }

    public record MediaSaveResult(string FileName, string Url);
}
