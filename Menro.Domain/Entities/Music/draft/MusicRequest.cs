using Menro.Domain.Entities.Music.Enums;

namespace Menro.Domain.Entities.Music.draft
{
    public class MusicRequest
    {
        public Guid Id { get; private set; }

        public Guid RestaurantId { get; private set; }

        public Guid MusicId { get; private set; }

        public string CustomerIdentifier { get; private set; } = string.Empty;

        public MusicRequestStatus Status { get; private set; }

        public DateTime RequestedAt { get; private set; }

        public DateTime? ReviewedAt { get; private set; }

        public string? RejectReason { get; private set; }
    }
}
