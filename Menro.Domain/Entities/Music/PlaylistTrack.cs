namespace Menro.Domain.Entities.Music
{
    public class PlaylistTrack
    {
        public Guid Id { get; set; }

        public Guid PlaylistId { get; set; }

        public Guid MusicTrackId { get; set; }

        public int SortOrder { get; set; }

        public bool IsRequestedTrack { get; set; }

        public Guid? TrackRequestId { get; set; }

        public Playlist Playlist { get; set; } = null!;
        public MusicTrack MusicTrack { get; set; } = null!;
    }
}