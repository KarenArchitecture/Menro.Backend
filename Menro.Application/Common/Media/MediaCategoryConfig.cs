namespace Menro.Application.Common.Media
{
    public sealed class MediaCategoryConfig
    {
        public required string FolderTemplate { get; init; }
        // {ownerId} توکن اختیاریه؛ اگه IsEntityScoped=true باشه باید پر بشه
        public required string[] AllowedExtensions { get; init; }
        public required long MaxSizeBytes { get; init; }
        public bool IsEntityScoped { get; init; }
        public bool IsPublic { get; init; } = true;
    }
}
