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

    }
}
