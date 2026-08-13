namespace Menro.Application.Features.Music.DTOs.Notifications
{
    public class TrackRequestedNotification
    {
        public Guid RequestId { get; set; }
        public Guid PlaylistTrackId { get; set; }
        public Guid MusicTrackId { get; set; }
        public string UserId { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime RequestedAt { get; set; }
    }
}