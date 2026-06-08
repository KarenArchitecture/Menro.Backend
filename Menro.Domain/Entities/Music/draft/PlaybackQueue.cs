namespace Menro.Domain.Entities.Music.draft
{
    public class PlaybackQueue
    {
        public Guid Id { get; private set; }

        public Guid RestaurantId { get; private set; }

        private readonly List<QueueItem> _items = [];
    }
}
