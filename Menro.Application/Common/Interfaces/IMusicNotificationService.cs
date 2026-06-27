namespace Menro.Application.Common.Interfaces
{
    public interface IMusicNotificationService
    {
        Task NotifyTrackRequested(int restaurantId, object payload);
    }
}
