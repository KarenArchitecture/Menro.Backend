namespace Menro.Application.Common.Media
{
    public sealed class MediaCategoryConfig
    {
        public required string FolderTemplate { get; init; }
        public required string[] AllowedExtensions { get; init; }
        public required long MaxSizeBytes { get; init; }
        public bool IsEntityScoped { get; init; }
        public bool IsPublic { get; init; } = true;
        public bool PreserveOriginalFileName { get; init; } = false;

        // Target width (px) for each generated variant. Null = don't generate that variant.
        // Height is always computed to preserve aspect ratio.
        // Leave both null for non-image categories (svg icons, audio files) -> no processing,
        // file is stored flat with no original/resized/thumbnail split.
        public int? ThumbnailWidth { get; init; }
        public int? ResizedWidth { get; init; }

        // True if this category should go through the image pipeline
        // (decode validation + variant generation + original/resized/thumbnail folder split).
        public bool IsImageProcessed => ThumbnailWidth.HasValue || ResizedWidth.HasValue;
    }
}
