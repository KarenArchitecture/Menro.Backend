using Menro.Application.Features.Music.DTOs.Notifications;
using Menro.Application.Features.Music.DTOs.Player;

namespace Menro.Application.Common.Interfaces
{
    public interface IMusicNotificationService
    {
        Task NotifyTrackRequested(int restaurantId, TrackRequestedNotification payload);
        Task NotifyTrackApproved(string userId, TrackApprovedNotification payload);
        Task NotifyTrackRejected(string userId, TrackRejectedNotification payload);
        Task NotifyPlaylistChanged(int restaurantId);
        Task NotifyPlaybackChanged(int restaurantId, MusicPlayerDto payload);
    }
}