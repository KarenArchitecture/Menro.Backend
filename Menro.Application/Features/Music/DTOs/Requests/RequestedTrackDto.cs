namespace Menro.Application.Features.Music.DTOs.Requests
{
    public class RequestedTrackDto
    {
        public Guid Id { get; set; }

        public Guid MusicTrackId { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }
    }
}
