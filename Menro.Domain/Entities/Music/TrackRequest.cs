using Menro.Domain.Entities.Music.Enums;

namespace Menro.Domain.Entities.Music
{
    public class TrackRequest
    {
        public Guid Id { get; set; }

        public int RestaurantId { get; set; }

        public Guid MusicTrackId { get; set; }

        public TrackRequestStatus Status { get; set; }

        public DateTime RequestedAt { get; set; }

        public MusicTrack MusicTrack { get; set; }
    }
}
