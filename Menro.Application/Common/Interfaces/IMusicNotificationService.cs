namespace Menro.Application.Common.Interfaces
{
    public interface IMusicNotificationService
    {
        Task NotifyTrackRequested(int restaurantId, object payload);
        Task NotifyTrackApproved(int restaurantId, object payload);
        Task NotifyTrackRejected(int restaurantId, object payload);
    }
}
