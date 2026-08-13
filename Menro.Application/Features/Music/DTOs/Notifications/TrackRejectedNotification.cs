namespace Menro.Application.Features.Music.DTOs.Notifications
{
    public class TrackRejectedNotification
    {
        public Guid RequestId { get; set; }
        public string? Reason { get; set; }
        public DateTime RejectedAt { get; set; }
    }
}