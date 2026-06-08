namespace Menro.Domain.Entities.Music.draft
{
    public class PlaybackSession
    {
        public Guid Id { get; private set; }

        public Guid RestaurantId { get; private set; }

        public Guid CurrentMusicId { get; private set; }

        public DateTime StartedAt { get; private set; }

        public bool IsPlaying { get; private set; }
    }
}
