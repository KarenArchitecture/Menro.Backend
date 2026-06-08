namespace Menro.Domain.Entities.Music
{
    public class PlaylistTrack
    {
        public Guid PlaylistId { get; private set; }

        public Guid MusicTrackId { get; private set; }

        public int SortOrder { get; private set; }

        public Playlist Playlist { get; private set; }

        public MusicTrack MusicTrack { get; private set; }
    }
}
