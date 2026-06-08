using Menro.Domain.Entities.Music.Enums;

namespace Menro.Domain.Entities.Music.draft
{
    public class QueueItem
    {
        public Guid Id { get; private set; }

        public Guid MusicId { get; private set; }

        public QueueItemType Type { get; private set; }

        public int Position { get; private set; }

        public DateTime CreatedAt { get; private set; }
    }
}
