namespace Menro.Domain.Entities.Music
{
    public class Playlist
    {
        public Guid Id { get; private set; }

        public Guid RestaurantId { get; private set; }

        public string Name { get; private set; }

        public bool IsActive { get; private set; }

        private readonly List<PlaylistTrack> _tracks = [];

        public IReadOnlyCollection<PlaylistTrack> Tracks => _tracks;
    }
}
