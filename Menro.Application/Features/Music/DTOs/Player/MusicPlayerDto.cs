namespace Menro.Application.Features.Music.DTOs.Player
{
    public class MusicPlayerDto
    {
        public Guid PlaylistId { get; set; }

        public Guid? CurrentPlaylistTrackId { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
