namespace Menro.Application.Features.Music.DTOs
{
    public class PlaylistDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public List<PlaylistTrackDto> Tracks { get; set; } = new List<PlaylistTrackDto>();
    }
}
