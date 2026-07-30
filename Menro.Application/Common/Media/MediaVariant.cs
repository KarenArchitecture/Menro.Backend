namespace Menro.Application.Common.Media
{
    // Represents which physical copy of a processed image we're pointing to.
    // Non-image categories (svg icons, audio files) only ever use Original.
    public enum MediaVariant
    {
        Original,
        Resized,
        Thumbnail
    }
}
