namespace Menro.Application.Features.Music.DTOs.Notifications
{
    public class TrackApprovedNotification
    {
        public Guid RequestId { get; set; }
        public Guid PlaylistTrackId { get; set; }
        public DateTime ApprovedAt { get; set; }
    }
}