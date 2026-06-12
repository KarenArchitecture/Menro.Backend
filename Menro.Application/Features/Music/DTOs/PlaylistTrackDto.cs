namespace Menro.Application.Features.Music.DTOs
{
    public class PlaylistTrackDto
    {
        public Guid Id { get; set; }
        public Guid MusicTrackId { get; set; }

        public string Title { get; set; }

        public string? Artist { get; set; }

        public string? CoverUrl { get; set; }

        public string AudioUrl { get; set; }

        public TimeSpan Duration { get; set; }

        public int SortOrder { get; set; }
    }
}
