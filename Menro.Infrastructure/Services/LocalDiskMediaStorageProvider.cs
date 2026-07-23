using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Services
{
    public class LocalDiskMediaStorageProvider : IMediaStorageProvider
    {
        private readonly MediaStorageOptions _options;

        public LocalDiskMediaStorageProvider(IOptions<MediaStorageOptions> options)
        {
            _options = options.Value;
        }

        public async Task<MediaSaveResult> SaveAsync(
            MediaCategory category, IFormFile file, string? ownerId = null,
            string? oldFileName = null, CancellationToken ct = default)
        {
            var cfg = MediaCategoryRegistry.All[category];
            ValidateFile(file, cfg);

            var folder = ResolveFolder(cfg, ownerId);
            var uploadDir = Path.Combine(_options.RootPath, folder);
            Directory.CreateDirectory(uploadDir);

            if (!string.IsNullOrEmpty(oldFileName))
                TryDeletePhysical(Path.Combine(uploadDir, Path.GetFileName(oldFileName)));

            string fileName;
            if (cfg.PreserveOriginalFileName)
            {
                fileName = Path.GetFileName(file.FileName);
                var existingPath = Path.Combine(uploadDir, fileName);
                if (File.Exists(existingPath))
                    throw new InvalidOperationException("فایلی با این نام از قبل وجود دارد.");
            }
            else
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                fileName = $"{Guid.NewGuid()}{ext}";
            }

            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            return new MediaSaveResult(fileName, GetUrl(category, fileName, ownerId));
        }

        public async Task<MediaSaveResult> SaveBytesAsync(
            MediaCategory category, byte[] bytes, string extension, string? ownerId = null, CancellationToken ct = default)
        {
            var cfg = MediaCategoryRegistry.All[category];
            var folder = ResolveFolder(cfg, ownerId);
            var uploadDir = Path.Combine(_options.RootPath, folder);
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            await File.WriteAllBytesAsync(Path.Combine(uploadDir, fileName), bytes, ct);

            return new MediaSaveResult(fileName, GetUrl(category, fileName, ownerId));
        }

        public bool Delete(MediaCategory category, string fileName, string? ownerId = null)
        {
            var cfg = MediaCategoryRegistry.All[category];
            var folder = ResolveFolder(cfg, ownerId);
            var path = Path.Combine(_options.RootPath, folder, Path.GetFileName(fileName));
            return TryDeletePhysical(path);
        }

        public string GetUrl(MediaCategory category, string fileName, string? ownerId = null)
        {
            var cfg = MediaCategoryRegistry.All[category];
            if (!cfg.IsPublic) return string.Empty;
            var folder = ResolveFolder(cfg, ownerId);
            var cleanPath = $"{folder}/{Path.GetFileName(fileName)}".Replace("\\", "/");
            return string.IsNullOrWhiteSpace(_options.BaseUrl) ? "/" + cleanPath : $"{_options.BaseUrl}/{cleanPath}";
        }
        public string GetBaseUrl() => _options.BaseUrl;

        public string GetPhysicalPath(MediaCategory category, string fileName, string? ownerId = null)
        {
            var cfg = MediaCategoryRegistry.All[category];
            var folder = ResolveFolder(cfg, ownerId);
            return Path.Combine(_options.RootPath, folder, Path.GetFileName(fileName));
        }


        /* --- HELPERS --- */
        private static string ResolveFolder(MediaCategoryConfig cfg, string? ownerId)
            => cfg.IsEntityScoped
                ? cfg.FolderTemplate.Replace("{ownerId}", ownerId)
                : cfg.FolderTemplate;

        private static void ValidateFile(IFormFile file, MediaCategoryConfig cfg)
        {
            if (file.Length == 0) throw new InvalidOperationException("فایل خالی است.");
            if (file.Length > cfg.MaxSizeBytes)
                throw new InvalidOperationException("حجم فایل بیش از حد مجاز است.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!cfg.AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("نوع فایل مجاز نیست.");
        }
        private static void ValidateBytes(byte[] data, string fileExtension, MediaCategoryConfig cfg)
        {
            if (data == null || data.Length == 0)
                throw new InvalidOperationException("فایل خالی است.");
            if (data.Length > cfg.MaxSizeBytes)
                throw new InvalidOperationException("حجم فایل بیش از حد مجاز است.");

            var ext = (fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}").ToLowerInvariant();
            if (!cfg.AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("نوع فایل مجاز نیست.");
        }

        private static bool TryDeletePhysical(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch { return false; }
        }
    }
}