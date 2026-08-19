using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Xml;
using System.Xml.Linq;

namespace Menro.Infrastructure.Services
{
    public class LocalDiskMediaStorageProvider : IMediaStorageProvider
    {

        #region DI
        private readonly MediaStorageOptions _options;

        public LocalDiskMediaStorageProvider(IOptions<MediaStorageOptions> options)
        {
            _options = options.Value;
        }
        #endregion

        #region configs
        private const int WebpQuality = 85;
        private const int MaxImagePixels = 40_000_000;
        // دسته‌های تصویری همیشه صرف‌نظر از فرمت آپلودی، به webp نرمالایز می‌شن.
        // دسته‌های غیرتصویری (svg، صدا) دست‌نخورده با همون پسوند آپلودی ذخیره می‌شن.
        private static string ResolveStorageExtension(MediaCategoryConfig cfg, string uploadedExt)
            => cfg.IsImageProcessed ? ".webp" : uploadedExt;
        private static readonly HashSet<string> DisallowedSvgElements = new(StringComparer.OrdinalIgnoreCase)
        {
            "script", "foreignObject", "iframe", "embed", "object", "use",
        };
        #endregion

        #region Main
        public async Task<MediaSaveResult> SaveAsync(
            MediaCategory category, IFormFile file, string? entityId = null,
            string? oldFileName = null, CancellationToken ct = default)
        {
            var cfg = MediaCategoryRegistry.All[category];
            ValidateFileMeta(file, cfg);

            byte[] fileBytes;
            await using (var inputStream = file.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                await inputStream.CopyToAsync(ms, ct);
                fileBytes = ms.ToArray();
            }

            ValidateContent(fileBytes, cfg);

            var uploadedExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storageExt = ResolveStorageExtension(cfg, uploadedExt);

            string fileName;
            if (cfg.PreserveOriginalFileName)
            {
                fileName = Path.GetFileName(file.FileName);
                var existingPath = Path.Combine(_options.RootPath, ResolveFolder(cfg, entityId, MediaVariant.Original), fileName);
                if (File.Exists(existingPath))
                    throw new InvalidOperationException("فایلی با این نام از قبل وجود دارد.");
            }
            else
            {
                fileName = $"{Guid.NewGuid()}{storageExt}";
            }

            await WriteVariantsAsync(cfg, fileBytes, fileName, entityId, ct);

            if (!string.IsNullOrEmpty(oldFileName))
                DeleteAllVariants(cfg, oldFileName, entityId);

            return new MediaSaveResult(fileName, GetUrl(category, fileName, entityId));
        }
        public async Task<MediaSaveResult> SaveBytesAsync(
            MediaCategory category, byte[] bytes, string extension, string? entityId = null, CancellationToken ct = default)
        {
            var cfg = MediaCategoryRegistry.All[category];
            var uploadedExt = (extension.StartsWith('.') ? extension : $".{extension}").ToLowerInvariant();

            ValidateBytesMeta(bytes, uploadedExt, cfg);
            ValidateContent(bytes, cfg);

            var storageExt = ResolveStorageExtension(cfg, uploadedExt);
            var fileName = $"{Guid.NewGuid()}{storageExt}";
            await WriteVariantsAsync(cfg, bytes, fileName, entityId, ct);

            return new MediaSaveResult(fileName, GetUrl(category, fileName, entityId));
        }
        public bool Delete(MediaCategory category, string fileName, string? entityId = null)
        {
            var cfg = MediaCategoryRegistry.All[category];
            return DeleteAllVariants(cfg, fileName, entityId);
        }

        public string GetUrl(MediaCategory category, string fileName, string? entityId = null, MediaVariant variant = MediaVariant.Original)
        {
            var cfg = MediaCategoryRegistry.All[category];
            if (!cfg.IsPublic) return string.Empty;
            var folder = ResolveFolder(cfg, entityId, variant);
            var cleanPath = $"{folder}/{Path.GetFileName(fileName)}".Replace("\\", "/");
            return string.IsNullOrWhiteSpace(_options.BaseUrl) ? "/" + cleanPath : $"{_options.BaseUrl}/{cleanPath}";
            //return "/" + cleanPath;
        }

        public string GetBaseUrl() => _options.BaseUrl;

        public string GetPhysicalPath(MediaCategory category, string fileName, string? entityId = null, MediaVariant variant = MediaVariant.Original)
        {
            var cfg = MediaCategoryRegistry.All[category];
            var folder = ResolveFolder(cfg, entityId, variant);
            return Path.Combine(_options.RootPath, folder, Path.GetFileName(fileName));
        }

        #endregion

        #region Folder resolution
        /* --- FOLDER RESOLUTION --- */

        // layout: {FolderTemplate}[/{entityId}][/original|resized|thumbnail]/{fileName}
        // the entityId segment only appears for entity-scoped categories,
        // the variant segment only appears for categories that go through image processing.
        private static string ResolveFolder(MediaCategoryConfig cfg, string? entityId, MediaVariant variant)
        {
            var path = cfg.FolderTemplate;

            if (cfg.IsEntityScoped)
            {
                if (string.IsNullOrWhiteSpace(entityId))
                    throw new InvalidOperationException("برای این دسته‌بندی، شناسه‌ی entity الزامی است.");
                path = $"{path}/{entityId}";
            }

            if (cfg.IsImageProcessed)
                path = $"{path}/{VariantFolderName(variant)}";

            return path;
        }

        private static string VariantFolderName(MediaVariant variant) => variant switch
        {
            MediaVariant.Original => "original",
            MediaVariant.Resized => "resized",
            MediaVariant.Thumbnail => "thumbnail",
            _ => "original"
        };

        private string EnsureDir(MediaCategoryConfig cfg, string? entityId, MediaVariant variant)
        {
            var dir = Path.Combine(_options.RootPath, ResolveFolder(cfg, entityId, variant));
            Directory.CreateDirectory(dir);
            return dir;
        }


        #endregion

        #region Write Pipeline
        /* --- WRITE PIPELINE --- */

        private async Task WriteVariantsAsync(MediaCategoryConfig cfg, byte[] bytes, string fileName, string? entityId, CancellationToken ct)
        {
            if (!cfg.IsImageProcessed)
            {
                var originalDir = EnsureDir(cfg, entityId, MediaVariant.Original);
                await File.WriteAllBytesAsync(Path.Combine(originalDir, fileName), bytes, ct);
                return;
            }

            // دسته‌های تصویری در همه‌ی واریانت‌ها (از جمله Original) به webp نرمالایز می‌شن،
            // صرف‌نظر از فرمتی که آپلود شده - برای یکدست بودن فرمت روی دیسک و صرفه‌جویی حجم در همه‌ی سایزها.
            using var bitmap = SKBitmap.Decode(bytes);
            if (bitmap == null)
                throw new InvalidOperationException("فایل تصویر معتبر نیست.");

            var originalDir2 = EnsureDir(cfg, entityId, MediaVariant.Original);
            SaveVariant(bitmap, bitmap.Width, Path.Combine(originalDir2, fileName));

            if (cfg.ResizedWidth.HasValue)
            {
                var dir = EnsureDir(cfg, entityId, MediaVariant.Resized);
                SaveVariant(bitmap, cfg.ResizedWidth.Value, Path.Combine(dir, fileName));
            }

            if (cfg.ThumbnailWidth.HasValue)
            {
                var dir = EnsureDir(cfg, entityId, MediaVariant.Thumbnail);
                SaveVariant(bitmap, cfg.ThumbnailWidth.Value, Path.Combine(dir, fileName));
            }
        }
        private static void SaveVariant(SKBitmap source, int targetWidth, string outputPath)
        {
            // هیچ‌وقت آپ‌اسکیل نکن: اگه اصل تصویر از تارگت کوچیک‌تره، همون سایز اصلی رو دوباره encode کن
            var width = Math.Min(targetWidth, source.Width);
            var height = (int)Math.Round(source.Height * (width / (double)source.Width));

            var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);
            using var resized = source.Resize(new SKImageInfo(width, height), samplingOptions)
                ?? throw new InvalidOperationException("خطا در تغییر اندازه‌ی تصویر.");
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
            using var fs = File.Create(outputPath);
            data.SaveTo(fs);
        }
        #endregion

        #region Delete
        /* --- DELETE --- */

        private bool DeleteAllVariants(MediaCategoryConfig cfg, string fileName, string? entityId)
        {
            var variants = cfg.IsImageProcessed
                ? new[] { MediaVariant.Original, MediaVariant.Resized, MediaVariant.Thumbnail }
                : new[] { MediaVariant.Original };

            var deletedAny = false;
            foreach (var variant in variants)
            {
                var path = Path.Combine(_options.RootPath, ResolveFolder(cfg, entityId, variant), Path.GetFileName(fileName));
                if (TryDeletePhysical(path)) deletedAny = true;
            }
            return deletedAny;
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
        #endregion

        #region Validation
        /* --- VALIDATION: metadata (size / claimed extension) --- */

        private static void ValidateFileMeta(IFormFile file, MediaCategoryConfig cfg)
        {
            if (file.Length == 0) throw new InvalidOperationException("فایل خالی است.");
            if (file.Length > cfg.MaxSizeBytes)
                throw new InvalidOperationException("حجم فایل بیش از حد مجاز است.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!cfg.AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("نوع فایل مجاز نیست.");
        }

        private static void ValidateBytesMeta(byte[] data, string extension, MediaCategoryConfig cfg)
        {
            if (data == null || data.Length == 0)
                throw new InvalidOperationException("فایل خالی است.");
            if (data.Length > cfg.MaxSizeBytes)
                throw new InvalidOperationException("حجم فایل بیش از حد مجاز است.");

            if (!cfg.AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("نوع فایل مجاز نیست.");
        }

        /* --- VALIDATION: actual content (security layer) --- */
        // Never trust the file extension alone. This checks the real bytes on disk match
        // what the category expects, regardless of what the client claims the file is.

        private static void ValidateContent(byte[] bytes, MediaCategoryConfig cfg)
        {
            if (cfg.IsImageProcessed)
            {
                using var codec = SKCodec.Create(new SKMemoryStream(bytes));
                if (codec == null)
                    throw new InvalidOperationException("فایل ارسالی یک تصویر معتبر نیست.");

                if ((long)codec.Info.Width * codec.Info.Height > MaxImagePixels)
                    throw new InvalidOperationException("ابعاد تصویر بیش از حد مجاز است.");

                using var bmp = SKBitmap.Decode(bytes);
                if (bmp == null)
                    throw new InvalidOperationException("فایل ارسالی یک تصویر معتبر نیست.");
                return;
            }

            if (cfg.AllowedExtensions.Contains(".svg"))
            {
                ValidateSvg(bytes);
                return;
            }

            ValidateAudioSignature(bytes);
        }
        private static void ValidateSvg(byte[] bytes)
        {
            string content;
            try { content = System.Text.Encoding.UTF8.GetString(bytes); }
            catch { throw new InvalidOperationException("فایل SVG معتبر نیست."); }

            var trimmed = content.TrimStart();
            if (!trimmed.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("فایل SVG معتبر نیست.");

            XDocument doc;
            try
            {
                using var stringReader = new StringReader(content);
                using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                });
                doc = XDocument.Load(xmlReader);
            }
            catch
            {
                throw new InvalidOperationException("فایل SVG معتبر نیست یا ساختار XML آن مخدوش است.");
            }

            foreach (var element in doc.Descendants())
            {
                if (DisallowedSvgElements.Contains(element.Name.LocalName))
                    throw new InvalidOperationException("فایل SVG شامل محتوای غیرمجاز است.");

                foreach (var attr in element.Attributes())
                {
                    if (attr.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("فایل SVG شامل محتوای غیرمجاز است.");

                    var value = attr.Value ?? string.Empty;
                    if (value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
                        value.Contains("data:text/html", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("فایل SVG شامل محتوای غیرمجاز است.");
                }
            }
        }
        private static readonly (byte[] Signature, int Offset)[] KnownAudioSignatures =
        {
            (new byte[] { 0x49, 0x44, 0x33 }, 0),             // MP3 with ID3 tag ("ID3")
            (new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0),       // WAV ("RIFF")
            (new byte[] { 0x4F, 0x67, 0x67, 0x53 }, 0),       // OGG ("OggS")
            (new byte[] { 0x66, 0x74, 0x79, 0x70 }, 4),       // M4A ("....ftyp")
        };

        private static void ValidateAudioSignature(byte[] bytes)
        {
            if (bytes.Length < 12)
                throw new InvalidOperationException("فایل صوتی معتبر نیست.");

            var matchesKnown = KnownAudioSignatures.Any(sig =>
                bytes.Length >= sig.Offset + sig.Signature.Length &&
                bytes.Skip(sig.Offset).Take(sig.Signature.Length).SequenceEqual(sig.Signature));

            // bare MP3 without an ID3 tag starts directly with a frame sync (11 set bits)
            var isBareMp3Frame = bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0;

            if (!matchesKnown && !isBareMp3Frame)
                throw new InvalidOperationException("فایل صوتی معتبر نیست یا فرمت آن پشتیبانی نمی‌شود.");
        }
        #endregion
    }
}