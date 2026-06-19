namespace Menro.Application.Features.Music.DTOs.Archive
{
    public class MusicTrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public string AudioUrl { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
    }
}
