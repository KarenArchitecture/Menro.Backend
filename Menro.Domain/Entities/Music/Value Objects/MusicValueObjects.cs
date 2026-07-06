namespace Menro.Domain.Entities.Music.Value_Objects
{
    public class MusicValueObjects
    {
        public record CustomerIdentifier(string Value);
        public record MusicFileInfo(string FilePath, string? CoverPath, TimeSpan Duration);

    }
}
