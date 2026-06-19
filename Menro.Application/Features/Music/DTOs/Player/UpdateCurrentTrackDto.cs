namespace Menro.Application.Features.Music.DTOs.Player
{
    public class UpdateCurrentTrackDto
    {
        public Guid PlaylistId { get; set; }

        public Guid PlaylistTrackId { get; set; }
    }
}
