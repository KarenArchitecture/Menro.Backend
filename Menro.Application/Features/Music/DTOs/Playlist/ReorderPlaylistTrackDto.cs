namespace Menro.Application.Features.Music.DTOs.Playlist
{
    public class ReorderPlaylistTrackDto
    {
        public PlaylistTrackMoveDirection Direction { get; set; }
    }

    public enum PlaylistTrackMoveDirection
    {
        Up = 1,
        Down = 2
    }
}
