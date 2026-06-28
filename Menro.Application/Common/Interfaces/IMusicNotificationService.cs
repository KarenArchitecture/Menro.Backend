namespace Menro.Application.Common.Interfaces
{
    public interface IMusicNotificationService
    {
        Task NotifyTrackRequested(int restaurantId, object payload);
        Task NotifyTrackApproved(string userId, object payload);
        Task NotifyTrackRejected(string userId, object payload);
    }
}
