namespace Menro.Application.Features.Music.DTOs.Playlist
{
    public class PlaylistItemDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public int Tracks { get; set; }
    }
}
