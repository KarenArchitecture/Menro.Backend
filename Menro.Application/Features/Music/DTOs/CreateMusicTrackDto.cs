namespace Menro.Application.Features.Music.DTOs
{
    public class CreateMusicTrackDto
    {
        public string Title { get; set; }

        public string Artist { get; set; }

        public string AudioFileName { get; set; }

        public string? CoverFileName { get; set; }
    }
}
