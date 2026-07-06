using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities.Music
{
    public class MusicPlayer
    {
        [Key]
        public int RestaurantId { get; set; }

        public Guid? PlaylistId { get; set; }

        public Guid? CurrentPlaylistTrackId { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public Playlist Playlist { get; set; }

        public PlaylistTrack? CurrentPlaylistTrack { get; set; }
    }
}