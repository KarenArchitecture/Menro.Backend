namespace Menro.Application.Features.Music.DTOs
{
    public class MusicTrackListItemDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public string? CoverFileName { get; set; }

        public bool IsActive { get; set; }
    }
}
