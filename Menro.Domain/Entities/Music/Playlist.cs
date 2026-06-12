namespace Menro.Domain.Entities.Music
{
    public class Playlist
    {
        public Guid Id { get; set; }

        public int RestaurantId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        private readonly List<PlaylistTrack> _tracks = [];

        public IReadOnlyCollection<PlaylistTrack> Tracks => _tracks;
    }
}
